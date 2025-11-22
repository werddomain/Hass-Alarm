using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Hass_Alarm.Data.Models
{
    public class PanelSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string SessionToken { get; set; }

        [Required]
        public string UserId { get; set; }

        public IdentityUser User { get; set; }

        [Required]
        public int DeviceId { get; set; }

        public Device Device { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public DateTime? LastActivityAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string IpAddress { get; set; }
    }
}
