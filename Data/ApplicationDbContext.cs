using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DailyPlanner.Models;
using System.Collections.Generic;


namespace DailyPlanner.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DailyPlan> DailyPlans { get; set; }
        public DbSet<DailyTask> DailyTasks { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }

        public DbSet<PlannedTask> PlannedTasks { get; set; }



    }
}
