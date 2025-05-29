using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DailyPlanner.Data;
using DailyPlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DailyPlanner.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var preferences = _context.UserPreferences.FirstOrDefault(p => p.UserId == user.Id) ?? new UserPreferences { UserId = user.Id };

            var tasks = _context.DailyTasks.Where(t => t.UserId == user.Id).ToList();
            var aiPlans = _context.PlannedTasks.Where(p => p.UserId == user.Id).OrderBy(p => p.StartTime).ToList();

            ViewBag.Tasks = tasks;
            ViewBag.AIPlans = aiPlans;

            // BURAYA ViewBag.Message = ... GELMEYECEK ❌

            return View(preferences);
        }


        [HttpPost]
        public async Task<IActionResult> Index(UserPreferences prefs)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = _context.UserPreferences.FirstOrDefault(p => p.UserId == user.Id);

            if (existing == null)
            {
                prefs.UserId = user.Id;
                _context.UserPreferences.Add(prefs);
            }
            else
            {
                existing.WakeUpTime = prefs.WakeUpTime;
                existing.SleepTime = prefs.SleepTime;
                existing.MealsPerDay = prefs.MealsPerDay;
                existing.DailyExerciseDuration = prefs.DailyExerciseDuration;
            }

            _context.SaveChanges();

            // SADECE BURADA AYARLANACAK ✅
            ViewBag.Message = "Tercihler başarıyla kaydedildi.";
            ViewBag.Tasks = _context.DailyTasks.Where(t => t.UserId == user.Id).ToList();
            ViewBag.AIPlans = _context.PlannedTasks.Where(p => p.UserId == user.Id).ToList();

            return View(prefs);
        }

    }
}
