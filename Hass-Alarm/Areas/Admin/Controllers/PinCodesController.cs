using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Hass_Alarm.Data;
using Hass_Alarm.Data.Models;

namespace Hass_Alarm.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PinCodesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PinCodesController(ApplicationDbContext context)
        {
            _context = context;
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
                _context.Add(pinCode);
                await _context.SaveChangesAsync();
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
    }
}
