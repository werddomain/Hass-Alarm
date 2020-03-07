using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hass_Alarm.Areas.Admin.Models.Users
{
    public class EditUserModel
    {
        public string UserId { get; set; }
        public bool Admin { get; set; }
        public bool Manager { get; set; }
        public bool Member { get; set; }
    }
}
