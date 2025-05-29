namespace DailyPlanner.Models
{
    public class DailyPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string DayName { get; set; } // Pazartesi, Salı...
        public DateTime DateCreated { get; set; }

        public ICollection<DailyTask> Tasks { get; set; }
    }

}
