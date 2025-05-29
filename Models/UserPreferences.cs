namespace DailyPlanner.Models
{
    public class UserPreferences
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public TimeSpan WakeUpTime { get; set; }
        public TimeSpan SleepTime { get; set; }
        public int MealsPerDay { get; set; }
        public TimeSpan DailyExerciseDuration { get; set; }
    }

}
