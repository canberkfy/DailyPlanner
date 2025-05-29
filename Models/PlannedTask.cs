using System;
using System.ComponentModel.DataAnnotations;

namespace DailyPlanner.Models
{
    public class PlannedTask
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
