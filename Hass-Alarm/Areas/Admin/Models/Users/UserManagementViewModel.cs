using System;
using Microsoft.AspNetCore.Identity;
using Hass_Alarm.Data.Models;

namespace Hass_Alarm.Areas.Admin.Models.Users
{
    public class UserManagementViewModel
    {
        public IdentityUser User { get; set; }
        public PinCode PinCode { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsManager { get; set; }
        public bool IsMember { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPowerUser { get; set; }
    }
}
