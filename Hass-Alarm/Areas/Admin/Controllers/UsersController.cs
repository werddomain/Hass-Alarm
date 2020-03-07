using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hass_Alarm.Areas.Admin.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Hass_Alarm.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Admin")]

    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration) {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            var users = _userManager.Users.ToArray();
            return View(users);
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
    }
}