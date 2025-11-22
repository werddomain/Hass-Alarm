using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hass_Alarm.Data;
using Hass_Alarm.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Hass_Alarm.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin,Manager")]
    public class PinCodesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Hass_Alarm.Services.IPinHashingService _pinHashingService;
        private readonly ILogger<PinCodesController> _logger;

        public PinCodesController(ApplicationDbContext context, Hass_Alarm.Services.IPinHashingService pinHashingService, ILogger<PinCodesController> logger)
        {
            _context = context;
            _pinHashingService = pinHashingService;
            _logger = logger;
        }

        // GET: Admin/PinCodes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PinCodes.Include(p => p.ActionGroup);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/PinCodes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pinCode = await _context.PinCodes
                .Include(p => p.ActionGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pinCode == null)
            {
                return NotFound();
            }

            return View(pinCode);
        }

        // GET: Admin/PinCodes/Create
        public IActionResult Create()
        {
            ViewData["ActionGroupId"] = new SelectList(_context.ActionGroups, "Id", "Name");
            return View();
        }

        // POST: Admin/PinCodes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Pin,Name,Enabled,ActionGroupId")] PinCode pinCode)
        {
            if (ModelState.IsValid)
            {
                // SECURITY: Hash the PIN before storing
                pinCode.Pin = _pinHashingService.HashPin(pinCode.Pin);

                _context.Add(pinCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created PIN {PinName} (hashed)", pinCode.Name);
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActionGroupId"] = new SelectList(_context.ActionGroups, "Id", "Name", pinCode.ActionGroupId);
            return View(pinCode);
        }

        // GET: Admin/PinCodes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pinCode = await _context.PinCodes.FindAsync(id);
            if (pinCode == null)
            {
                return NotFound();
            }
            ViewData["ActionGroupId"] = new SelectList(_context.ActionGroups, "Id", "Name", pinCode.ActionGroupId);
            return View(pinCode);
        }

        // POST: Admin/PinCodes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Pin,Name,Enabled,ActionGroupId")] PinCode pinCode)
        {
            if (id != pinCode.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // SECURITY: Hash the PIN before storing (if it's not already hashed)
                    if (!_pinHashingService.IsHashed(pinCode.Pin))
                    {
                        pinCode.Pin = _pinHashingService.HashPin(pinCode.Pin);
                        _logger.LogInformation("Updated PIN {PinId} (hashed)", pinCode.Id);
                    }
                    else
                    {
                        _logger.LogInformation("Updated PIN {PinId} (already hashed)", pinCode.Id);
                    }

                    _context.Update(pinCode);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PinCodeExists(pinCode.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActionGroupId"] = new SelectList(_context.ActionGroups, "Id", "Id", pinCode.ActionGroupId);
            return View(pinCode);
        }

        // GET: Admin/PinCodes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pinCode = await _context.PinCodes
                .Include(p => p.ActionGroup)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pinCode == null)
            {
                return NotFound();
            }

            return View(pinCode);
        }

        // POST: Admin/PinCodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pinCode = await _context.PinCodes.FindAsync(id);
            _context.PinCodes.Remove(pinCode);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PinCodeExists(int id)
        {
            return _context.PinCodes.Any(e => e.Id == id);
        }

        // GET: Admin/PinCodes/MigratePins
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MigratePins()
        {
            var allPins = await _context.PinCodes.ToListAsync();
            var unhashedPins = allPins.Where(p => !_pinHashingService.IsHashed(p.Pin)).ToList();

            ViewBag.TotalPins = allPins.Count;
            ViewBag.UnhashedPins = unhashedPins.Count;
            ViewBag.HashedPins = allPins.Count - unhashedPins.Count;

            return View();
        }

        // POST: Admin/PinCodes/MigratePins
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MigratePinsConfirm()
        {
            var allPins = await _context.PinCodes.ToListAsync();
            int migratedCount = 0;
            int errorCount = 0;

            foreach (var pin in allPins)
            {
                // Check if PIN is already hashed
                if (!_pinHashingService.IsHashed(pin.Pin))
                {
                    try
                    {
                        // Hash the plain text PIN
                        var originalPin = pin.Pin;
                        pin.Pin = _pinHashingService.HashPin(originalPin);
                        migratedCount++;

                        _logger.LogInformation("Migrated PIN {PinId} ({PinName}) from plain text to hashed", pin.Id, pin.Name);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, "Error migrating PIN {PinId} ({PinName})", pin.Id, pin.Name);
                    }
                }
            }

            if (migratedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("PIN Migration completed: {MigratedCount} PINs hashed, {ErrorCount} errors", migratedCount, errorCount);
            }

            TempData["Success"] = $"Migration completed: {migratedCount} PINs hashed successfully. {errorCount} errors occurred.";
            return RedirectToAction(nameof(Index));
        }
    }
}
