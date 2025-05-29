using Microsoft.AspNetCore.Mvc;

namespace DailyPlanner.Controllers
{
    public class PlanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
