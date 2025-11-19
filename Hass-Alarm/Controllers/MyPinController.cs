using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hass_Alarm.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hass_Alarm.Controllers
{
    public class MyPinController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<MyPinController> _logger;

        public MyPinController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, ILogger<MyPinController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // MEDIUM FIX: Use async query
            var pin = await _dbContext.PinCodes.FirstOrDefaultAsync(o => o.UserId == myUserId);

            return View(pin);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Data.Models.PinCode model)
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // MEDIUM FIX: Validate PIN format
            if (string.IsNullOrWhiteSpace(model.Pin))
            {
                ModelState.AddModelError("Pin", "PIN code is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.Pin, @"^\d{4,8}$"))
            {
                ModelState.AddModelError("Pin", "PIN code must be 4-8 digits.");
            }

            // MEDIUM FIX: Check for duplicate PIN (used by another user)
            var duplicatePin = await _dbContext.PinCodes
                .AnyAsync(p => p.Pin == model.Pin && p.UserId != myUserId);
            if (duplicatePin)
            {
                ModelState.AddModelError("Pin", "This PIN code is already in use by another user.");
            }

            if (ModelState.IsValid)
            {
                // MEDIUM FIX: Use async query
                var pin = await _dbContext.PinCodes.FirstOrDefaultAsync(o => o.UserId == myUserId);

                if (pin == null)
                {
                    // Create new PIN
                    await _dbContext.PinCodes.AddAsync(new Data.Models.PinCode
                    {
                        Name = model.Name,
                        ActionGroupId = model.ActionGroupId,
                        Enabled = model.Enabled,
                        Pin = model.Pin,
                        UserId = myUserId
                    });

                    _logger.LogInformation("User {UserId} created a new PIN", myUserId);
                    TempData["Success"] = "Your PIN has been created successfully.";
                }
                else
                {
                    // Update existing PIN
                    pin.Name = model.Name;
                    pin.Pin = model.Pin;
                    pin.Enabled = model.Enabled;
                    pin.ActionGroupId = model.ActionGroupId;

                    _logger.LogInformation("User {UserId} updated their PIN", myUserId);
                    TempData["Success"] = "Your PIN has been updated successfully.";
                }

                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(model);
        }

    }
}