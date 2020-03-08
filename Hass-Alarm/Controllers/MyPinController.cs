using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hass_Alarm.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hass_Alarm.Controllers
{
    public class MyPinController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public MyPinController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pin = _dbContext.PinCodes.FirstOrDefault(o => o.UserId == myUserId);

            return View(pin);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Data.Models.PinCode model)
        {
            if (ModelState.IsValid)
            {
                var myUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var pin = _dbContext.PinCodes.FirstOrDefault(o => o.UserId == myUserId);
                if (pin == null)
                {
                    var d = await _dbContext.PinCodes.AddAsync(new Data.Models.PinCode
                    {
                        Name = model.Name,
                        ActionGroupId = model.ActionGroupId,
                        Enabled = model.Enabled,
                        Pin = model.Pin,
                        UserId = myUserId
                    });

                }
                else
                {
                    pin.Name = model.Name;
                    pin.Pin = model.Pin;
                    pin.Enabled = model.Enabled;
                    pin.ActionGroupId = model.ActionGroupId;

                }
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

    }
}