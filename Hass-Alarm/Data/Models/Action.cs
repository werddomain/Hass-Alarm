using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Hass_Alarm.Data.Models
{
    public class Action
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ActionGroupId { get; set; }

        public string Name { get; set; }

        public string Input_Boolean_EntityId { get; set; }
        public bool Input_Boolean_SetState { get; set; }

        public string Service_Domain { get; set; }
        public string Service_Name { get; set; }
        public string Service_Fields { get; set; }


    }
}
