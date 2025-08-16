using Microsoft.AspNetCore.Mvc;

namespace PjPRN222.Controllers
{
    public class HomeAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
