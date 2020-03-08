using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hass_Alarm.Data.Models
{
    public class PinCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public string Pin { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public bool Enabled { get; set; }

        public string UserId { get; set; }

        public int ActionGroupId { get; set; }
        public ActionGroup ActionGroup { get; set; }



    }
}
