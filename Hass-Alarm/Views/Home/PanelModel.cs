using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Hass_Alarm.Views.Home
{
    public class PanelModel
    {
        public ArmState ArmState { get; set; }
        public string State { get; set; }

        [Required]
        public string code { get; set; }

        [Required]
        public string action { get; set; }

        public bool code_invalid { get; set; }

    }
}
