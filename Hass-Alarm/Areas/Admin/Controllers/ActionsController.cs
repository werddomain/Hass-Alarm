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
    public class ActionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Actions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actions.ToListAsync());
        }

        // GET: Admin/Actions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var action = await _context.Actions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (action == null)
            {
                return NotFound();
            }

            return View(action);
        }

        // GET: Admin/Actions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Actions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ActionGroupId,Name,Input_Boolean_EntityId,Input_Boolean_SetState,Service_Domain,Service_Name,Service_Fields")] Data.Models.Action action)
        {
            if (ModelState.IsValid)
            {
                _context.Add(action);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(action);
        }

        // GET: Admin/Actions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var action = await _context.Actions.FindAsync(id);
            if (action == null)
            {
                return NotFound();
            }
            return View(action);
        }

        // POST: Admin/Actions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ActionGroupId,Name,Input_Boolean_EntityId,Input_Boolean_SetState,Service_Domain,Service_Name,Service_Fields")] Data.Models.Action action)
        {
            if (id != action.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(action);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActionExists(action.Id))
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
            return View(action);
        }

        // GET: Admin/Actions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var action = await _context.Actions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (action == null)
            {
                return NotFound();
            }

            return View(action);
        }

        // POST: Admin/Actions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var action = await _context.Actions.FindAsync(id);
            _context.Actions.Remove(action);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActionExists(int id)
        {
            return _context.Actions.Any(e => e.Id == id);
        }
    }
}
