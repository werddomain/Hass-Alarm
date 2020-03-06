using System;
using System.Collections.Generic;
using System.Text;
using Hass_Alarm.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hass_Alarm.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<PinCode> PinCodes { get; set; }
        public DbSet<Models.Action> Actions { get; set; }
        public DbSet<ActionGroup> ActionGroups { get; set; }


    }
}
