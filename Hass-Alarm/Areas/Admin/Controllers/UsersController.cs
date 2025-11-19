using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hass_Alarm.Areas.Admin.Models.Users;
using Hass_Alarm.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hass_Alarm.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]

    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        private bool IsPowerUser(IdentityUser user)
        {
            var powerUserEmail = _configuration["Admin:Email"];
            var powerUserName = _configuration["Admin:UserName"];

            return (user.Email != null && powerUserEmail != null && user.Email.Equals(powerUserEmail, StringComparison.OrdinalIgnoreCase)) ||
                   (user.UserName != null && powerUserName != null && user.UserName.Equals(powerUserName, StringComparison.OrdinalIgnoreCase));
        }
        public async Task<IActionResult> Index()
        {
            // Load all users asynchronously
            var users = await _userManager.Users.ToListAsync();

            // Load all PIN codes in one query to avoid N+1 problem
            var userIds = users.Select(u => u.Id).ToList();
            var pinCodes = await _context.PinCodes
                .Where(p => userIds.Contains(p.UserId))
                .ToListAsync();
            var pinCodesByUserId = pinCodes.ToDictionary(p => p.UserId);

            var viewModels = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                // Get PIN code from dictionary (no database query)
                pinCodesByUserId.TryGetValue(user.Id, out var pinCode);

                // Get roles
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isManager = await _userManager.IsInRoleAsync(user, "Manager");
                var isMember = await _userManager.IsInRoleAsync(user, "Member");

                viewModels.Add(new UserManagementViewModel
                {
                    User = user,
                    PinCode = pinCode,
                    IsAdmin = isAdmin,
                    IsManager = isManager,
                    IsMember = isMember,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    IsPowerUser = IsPowerUser(user)
                });
            }

            return View(viewModels);
        }

        public async Task<IActionResult> EditAsync(string id)
        {
            // MEDIUM FIX: Validate parameter
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("EditAsync called with null or empty id");
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Attempt to edit non-existent user with ID: {UserId}", id);
                return NotFound();
            }

            ViewData["User"] = user;
            var model = new EditUserModel();
            model.UserId = id;
            model.Admin = await _userManager.IsInRoleAsync(user, "Admin");
            model.Manager = await _userManager.IsInRoleAsync(user, "Manager");
            model.Member = await _userManager.IsInRoleAsync(user, "Member");
            return View(model);
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> EditAsync(EditUserModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                _logger.LogWarning("Attempt to edit non-existent user with ID: {UserId}", model.UserId);
                return NotFound();
            }

            var admin = await _userManager.IsInRoleAsync(user, "Admin");
            var manager = await _userManager.IsInRoleAsync(user, "Manager");
            var member = await _userManager.IsInRoleAsync(user, "Member");

            if (model.Admin != admin)
            {
                if (model.Admin)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    _logger.LogInformation("Added Admin role to user {UserName}", user.UserName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    _logger.LogInformation("Removed Admin role from user {UserName}", user.UserName);
                }
            }

            if (model.Manager != manager)
            {
                if (model.Manager)
                {
                    await _userManager.AddToRoleAsync(user, "Manager");
                    _logger.LogInformation("Added Manager role to user {UserName}", user.UserName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "Manager");
                    _logger.LogInformation("Removed Manager role from user {UserName}", user.UserName);
                }
            }

            if (model.Member != member)
            {
                if (model.Member)
                {
                    await _userManager.AddToRoleAsync(user, "Member");
                    _logger.LogInformation("Added Member role to user {UserName}", user.UserName);
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "Member");
                    _logger.LogInformation("Removed Member role from user {UserName}", user.UserName);
                }
            }

            TempData["Success"] = $"User {user.UserName} roles updated successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteAsync(string id)
        {
            // MEDIUM FIX: Validate parameter
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("DeleteAsync called with null or empty id");
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Attempt to delete non-existent user with ID: {UserId}", id);
                return NotFound();
            }

            if (IsPowerUser(user))
            {
                _logger.LogWarning("Attempt to delete power user {UserName}", user.UserName);
                return RedirectToAction("Index");
            }

            return View(user);
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> ConfirmDeleteAsync(string id)
        {
            // MEDIUM FIX: Validate parameter
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("ConfirmDeleteAsync called with null or empty id");
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Attempt to confirm delete for non-existent user with ID: {UserId}", id);
                return NotFound();
            }

            if (IsPowerUser(user))
            {
                _logger.LogWarning("Attempt to confirm delete for power user {UserName}", user.UserName);
                TempData["Error"] = "Cannot delete power user.";
                return RedirectToAction("Index");
            }

            var userName = user.UserName;
            await _userManager.DeleteAsync(user);
            _logger.LogInformation("Deleted user {UserName} (ID: {UserId})", userName, id);
            TempData["Success"] = $"User {userName} deleted successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(string id)
        {
            // MEDIUM FIX: Validate parameter
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("ToggleUserLock called with null or empty id");
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Attempt to toggle lock for non-existent user with ID: {UserId}", id);
                return NotFound();
            }

            if (IsPowerUser(user))
            {
                _logger.LogWarning("Attempt to toggle lock for power user {UserName}", user.UserName);
                TempData["Error"] = "Cannot lock/unlock power user.";
                return RedirectToAction("Index");
            }

            var isCurrentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            if (isCurrentlyLocked)
            {
                // Unlock the user
                await _userManager.SetLockoutEndDateAsync(user, null);
                _logger.LogInformation("Unlocked user {UserName} (ID: {UserId})", user.UserName, user.Id);
                TempData["Success"] = $"User {user.UserName} unlocked successfully.";
            }
            else
            {
                // Enable lockout and lock the user indefinitely
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                _logger.LogInformation("Locked user {UserName} (ID: {UserId})", user.UserName, user.Id);
                TempData["Success"] = $"User {user.UserName} locked successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePinEnabled(string userId)
        {
            // MEDIUM FIX: Validate parameter
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("TogglePinEnabled called with null or empty userId");
                return BadRequest();
            }

            var pinCode = await _context.PinCodes.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pinCode == null)
            {
                _logger.LogWarning("Attempt to toggle PIN for user with no PIN code. UserId: {UserId}", userId);
                TempData["Error"] = "User does not have a PIN code.";
                return NotFound();
            }

            pinCode.Enabled = !pinCode.Enabled;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled PIN {PinName} to {Status} for user {UserId}",
                pinCode.Name, pinCode.Enabled ? "Enabled" : "Disabled", userId);

            TempData["Success"] = $"PIN {pinCode.Name} {(pinCode.Enabled ? "enabled" : "disabled")} successfully.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreatePin(string userId)
        {
            // MEDIUM FIX: Validate parameter
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("CreatePin called with null or empty userId");
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Attempt to create PIN for non-existent user with ID: {UserId}", userId);
                return NotFound();
            }

            var existingPin = await _context.PinCodes.FirstOrDefaultAsync(p => p.UserId == userId);
            if (existingPin != null)
            {
                _logger.LogWarning("Attempt to create duplicate PIN for user {UserName} who already has PIN", user.UserName);
                TempData["Error"] = "This user already has a PIN code.";
                return RedirectToAction("Index");
            }

            ViewData["User"] = user;
            ViewData["ActionGroupId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ActionGroups, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePin([Bind("Pin,Name,Enabled,ActionGroupId,UserId")] Data.Models.PinCode pinCode)
        {
            // Validate PIN format
            if (string.IsNullOrWhiteSpace(pinCode.Pin))
            {
                ModelState.AddModelError("Pin", "PIN code is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(pinCode.Pin, @"^\d{4,8}$"))
            {
                ModelState.AddModelError("Pin", "PIN code must be 4-8 digits.");
            }

            // Check for duplicate PIN
            var duplicatePin = await _context.PinCodes
                .AnyAsync(p => p.Pin == pinCode.Pin && p.UserId != pinCode.UserId);
            if (duplicatePin)
            {
                ModelState.AddModelError("Pin", "This PIN code is already in use by another user.");
            }

            // Check if user already has a PIN
            var existingUserPin = await _context.PinCodes
                .AnyAsync(p => p.UserId == pinCode.UserId);
            if (existingUserPin)
            {
                ModelState.AddModelError("", "This user already has a PIN code.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(pinCode);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created PIN {PinName} for user {UserId}", pinCode.Name, pinCode.UserId);
                TempData["Success"] = $"PIN {pinCode.Name} created successfully.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(pinCode.UserId);
            if (user == null)
            {
                _logger.LogError("User not found when returning CreatePin view. UserId: {UserId}", pinCode.UserId);
                return NotFound();
            }

            ViewData["User"] = user;
            ViewData["ActionGroupId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ActionGroups, "Id", "Name", pinCode.ActionGroupId);
            return View(pinCode);
        }
    }
}