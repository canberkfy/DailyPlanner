using DailyPlanner.Models;

namespace DailyPlanner.Services
{
    public class SmartPlanner
    {
        public static List<PlannedTask> CreatePlan(UserPreferences prefs, List<DailyTask> tasks)
        {
            var plannedTasks = new List<PlannedTask>();

            if (prefs == null || prefs.SleepTime <= prefs.WakeUpTime || tasks == null || tasks.Count == 0)
                return plannedTasks;

            var current = prefs.WakeUpTime;
            TimeSpan totalAvailable = prefs.SleepTime > prefs.WakeUpTime
    ? prefs.SleepTime - prefs.WakeUpTime
    : TimeSpan.FromHours(24) - prefs.WakeUpTime + prefs.SleepTime;

            var taskDuration = totalAvailable.TotalMinutes / tasks.Count;

            foreach (var task in tasks)
            {
                var start = current;
                var end = current.Add(TimeSpan.FromMinutes(taskDuration));

                plannedTasks.Add(new PlannedTask
                {
                    Description = task.Description,
                    StartTime = start,
                    EndTime = end,
                    UserId = task.UserId,
                    CreatedAt = DateTime.Now
                });

                current = end;
            }

            return plannedTasks;
        }
    }
}
