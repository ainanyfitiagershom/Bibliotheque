using Microsoft.AspNetCore.Mvc;

namespace Frontoffice.MVC.Controllers
{
    public class AideController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }
    }
}
