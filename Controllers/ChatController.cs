using DailyPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using DailyPlanner.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DailyPlanner.Services;
using System.Text.RegularExpressions;

namespace DailyPlanner.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GptService _gpt;

        public ChatController(ApplicationDbContext context, UserManager<IdentityUser> userManager, GptService gpt)
        {
            _context = context;
            _userManager = userManager;
            _gpt = gpt;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var prefs = _context.UserPreferences.FirstOrDefault(p => p.UserId == user.Id);
            var tasks = _context.DailyTasks.Where(t => t.UserId == user.Id).ToList();

            var plan = SmartPlanner.CreatePlan(prefs, tasks);
            ViewBag.Plan = plan;
            return View(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> AddTask(string description)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrWhiteSpace(description))
            {
                _context.DailyTasks.Add(new DailyTask
                {
                    UserId = user.Id,
                    Description = description,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.DailyTasks.FindAsync(id);
            if (task != null)
            {
                _context.DailyTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePlanFromText(string taskText)
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrWhiteSpace(taskText))
                return RedirectToAction("Index");

            var tasks = new List<DailyTask>();

            foreach (var raw in taskText.Split(','))
            {
                var trimmed = raw.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                int durationMinutes = 60; // Default duration
                var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"\((\d+)\s*(dk|dakika|saat)?\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    int.TryParse(match.Groups[1].Value, out durationMinutes);
                    if (match.Groups[2].Value.ToLower().Contains("saat"))
                        durationMinutes *= 60;

                    trimmed = System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s*\(.*?\)", "").Trim();
                }

                tasks.Add(new DailyTask
                {
                    UserId = user.Id,
                    Description = trimmed,
                    DurationMinutes = durationMinutes,
                    CreatedAt = DateTime.Now
                });
            }

            _context.DailyTasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GeneratePlan()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                var preferences = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);
                var taskEntities = _context.DailyTasks.Where(t => t.UserId == user.Id).ToList();
                var taskDescriptions = taskEntities.Select(t => t.Description).ToList();

                string prompt = preferences != null
                    ? $"Günlük yapılacaklar listesi: {string.Join(", ", taskDescriptions)}. Sabah saat {preferences.WakeUpTime.Hours:D2}:{preferences.WakeUpTime.Minutes:D2}'de kalkıyorum, gece {preferences.SleepTime.Hours:D2}:{preferences.SleepTime.Minutes:D2}'te yatıyorum. Günde {preferences.MealsPerDay} öğün yiyorum. Spor süresi {preferences.DailyExerciseDuration.TotalMinutes} dakika. Her öğünün arası en az 4 saat olmalı. Aralara mola da ekle. Lütfen saatleri 00:00 ile 23:59 arası olacak şekilde yaz."
                    : $"Günlük yapılacaklar listesi: {string.Join(", ", taskDescriptions)}. Sabah saat 08:00'de kalkıyorum, gece 23:00'te yatıyorum. Her öğünün arası en az 4 saat olmalı. Aralara mola da ekle. Lütfen saatleri 00:00 ile 23:59 arası olacak şekilde yaz.";

                // Prompt dosyaya yazılsın (debug için)
                string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs");
                Directory.CreateDirectory(logsPath);
                System.IO.File.WriteAllText(Path.Combine(logsPath, "gptPrompt.txt"), prompt);

                string gptResponse = await _gpt.GetOptimizedScheduleAsync(prompt);

                if (string.IsNullOrWhiteSpace(gptResponse))
                {
                    TempData["Error"] = "Yapay zekadan bir cevap alınamadı.";
                    return RedirectToAction("Index");
                }

                // Cevap dosyaya yazılsın
                System.IO.File.WriteAllText(Path.Combine(logsPath, "gptResponse.txt"), gptResponse);

                var plannedTasks = new List<PlannedTask>();
                var lines = gptResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var maxTime = new TimeSpan(23, 59, 59);

                foreach (var line in lines)
                {
                    // Satırdan zaman aralığı ve açıklamayı ayıklama
                    var match = Regex.Match(line, @"\*\*(?<start>\d{1,2}[:.]\d{2})\s*-\s*(?<end>\d{1,2}[:.]\d{2})\*\*\s*[:：]?\s*(?<desc>.+)");

                    if (match.Success)
                    {
                        string Normalize(string t) => t.Contains(":") ? t.Replace('.', ':') : $"{t}:00";

                        if (TimeSpan.TryParse(Normalize(match.Groups["start"].Value), out var start) &&
                            TimeSpan.TryParse(Normalize(match.Groups["end"].Value), out var end) &&
                            start <= maxTime && end <= maxTime)
                        {
                            plannedTasks.Add(new PlannedTask
                            {
                                UserId = user.Id,
                                StartTime = start,
                                EndTime = end,
                                Description = match.Groups["desc"].Value.Trim(),
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                }

                if (plannedTasks.Any())
                {
                    _context.PlannedTasks.AddRange(plannedTasks);
                    await _context.SaveChangesAsync();
                    return View("PlanResult", plannedTasks);
                }
                else
                {
                    TempData["Error"] = "Yapay zekadan gelen cevap uygun formatta değil veya hiç görev ayrıştırılamadı.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }






        [HttpPost]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var plan = await _context.PlannedTasks.FindAsync(id);

            if (plan != null && plan.UserId == user.Id)
            {
                _context.PlannedTasks.Remove(plan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Profile");
        }
    }
}
