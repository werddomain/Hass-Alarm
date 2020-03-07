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

namespace Hass_Alarm.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin,Manager")]
    public class ActionGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActionGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ActionGroups
        public async Task<IActionResult> Index()
        {
            return View(await _context.ActionGroups.ToListAsync());
        }

        // GET: Admin/ActionGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actionGroup = await _context.ActionGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actionGroup == null)
            {
                return NotFound();
            }

            return View(actionGroup);
        }

        // GET: Admin/ActionGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ActionGroups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ActionGroup actionGroup)
        {
            if (ModelState.IsValid)
            {
                _context.Add(actionGroup);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actionGroup);
        }

        // GET: Admin/ActionGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actionGroup = await _context.ActionGroups.FindAsync(id);
            if (actionGroup == null)
            {
                return NotFound();
            }
            return View(actionGroup);
        }

        // POST: Admin/ActionGroups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] ActionGroup actionGroup)
        {
            if (id != actionGroup.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(actionGroup);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActionGroupExists(actionGroup.Id))
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
            return View(actionGroup);
        }

        // GET: Admin/ActionGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actionGroup = await _context.ActionGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actionGroup == null)
            {
                return NotFound();
            }

            return View(actionGroup);
        }

        // POST: Admin/ActionGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actionGroup = await _context.ActionGroups.FindAsync(id);
            _context.ActionGroups.Remove(actionGroup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActionGroupExists(int id)
        {
            return _context.ActionGroups.Any(e => e.Id == id);
        }
    }
}
