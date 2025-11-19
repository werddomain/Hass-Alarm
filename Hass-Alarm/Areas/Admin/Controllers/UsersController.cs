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

namespace Hass_Alarm.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]

    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, ApplicationDbContext context) {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var powerUserEmail = _configuration["Admin:Email"];
            var powerUserName = _configuration["Admin:UserName"];

            var viewModels = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var pinCode = await _context.PinCodes.FirstOrDefaultAsync(p => p.UserId == user.Id);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isManager = await _userManager.IsInRoleAsync(user, "Manager");
                var isMember = await _userManager.IsInRoleAsync(user, "Member");
                var isPowerUser = user.Email?.ToLower() == powerUserEmail?.ToLower() ||
                                  user.UserName?.ToLower() == powerUserName?.ToLower();

                viewModels.Add(new UserManagementViewModel
                {
                    User = user,
                    PinCode = pinCode,
                    IsAdmin = isAdmin,
                    IsManager = isManager,
                    IsMember = isMember,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    IsPowerUser = isPowerUser
                });
            }

            return View(viewModels);
        }

        public async Task<IActionResult> EditAsync(string id) {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            ViewData["User"] = user;
            var model = new EditUserModel();
            model.UserId = id;
            model.Admin = await _userManager.IsInRoleAsync(user, "Admin");
            model.Manager = await _userManager.IsInRoleAsync(user, "Manager");
            model.Member = await _userManager.IsInRoleAsync(user, "Member");
            return View(model);
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> EditAsync(EditUserModel model) {
            var user = await _userManager.FindByIdAsync(model.UserId);
            var admin = await _userManager.IsInRoleAsync(user, "Admin");
            var manager = await _userManager.IsInRoleAsync(user, "Manager");
            var member = await _userManager.IsInRoleAsync(user, "Member");
            if (model.Admin != admin) {
                if (model.Admin)
                    await _userManager.AddToRoleAsync(user, "Admin");
                else
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            if (model.Manager != manager) {
                if (model.Manager)
                    await _userManager.AddToRoleAsync(user, "Manager");
                else
                    await _userManager.RemoveFromRoleAsync(user, "Manager");
            }
            if (model.Member != member) {
                if (model.Member)
                    await _userManager.AddToRoleAsync(user, "Member");
                else
                    await _userManager.RemoveFromRoleAsync(user, "Member");
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var poweruser = new IdentityUser
            {

                UserName = _configuration["Admin:UserName"],
                Email = _configuration["Admin:Email"],
            };
            if (user.Email.ToLower() == poweruser.Email.ToLower() || user.UserName.ToLower() == poweruser.UserName.ToLower())
            {
                return RedirectToAction("Index");
            }
            
            return View(user);
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> ConfirmDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var poweruser = new IdentityUser
            {

                UserName = _configuration["Admin:UserName"],
                Email = _configuration["Admin:Email"],
            };
            if (user.Email.ToLower() == poweruser.Email.ToLower() || user.UserName.ToLower() == poweruser.UserName.ToLower())
            {
                return RedirectToAction("Index");
            }
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var powerUserEmail = _configuration["Admin:Email"];
            var powerUserName = _configuration["Admin:UserName"];
            var isPowerUser = user.Email?.ToLower() == powerUserEmail?.ToLower() ||
                              user.UserName?.ToLower() == powerUserName?.ToLower();

            if (isPowerUser)
            {
                return RedirectToAction("Index");
            }

            var isCurrentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            if (isCurrentlyLocked)
            {
                // Unlock the user
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                // Lock the user indefinitely
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePinEnabled(string userId)
        {
            var pinCode = await _context.PinCodes.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pinCode == null)
            {
                return NotFound();
            }

            pinCode.Enabled = !pinCode.Enabled;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult CreatePin(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
                return NotFound();

            var existingPin = _context.PinCodes.FirstOrDefault(p => p.UserId == userId);
            if (existingPin != null)
            {
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
            if (ModelState.IsValid)
            {
                _context.Add(pinCode);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(pinCode.UserId);
            ViewData["User"] = user;
            ViewData["ActionGroupId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ActionGroups, "Id", "Name", pinCode.ActionGroupId);
            return View(pinCode);
        }
    }
}