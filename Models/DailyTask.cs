﻿namespace DailyPlanner.Models
{
    public class DailyTask
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int DurationMinutes { get; set; }

    }
}

