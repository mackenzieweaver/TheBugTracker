using Microsoft.AspNetCore.Mvc;

namespace TheBugTracker.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
