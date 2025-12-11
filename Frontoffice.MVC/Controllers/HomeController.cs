using Frontoffice.MVC.Models;
using Frontoffice.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Frontoffice.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILivreService _livreService;

        public HomeController(ILivreService livreService)
        {
            _livreService = livreService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                Nouveautes = await _livreService.GetNouveautesAsync(8),
                Populaires = await _livreService.GetPopulairesAsync(8),
                Categories = await _livreService.GetCategoriesAsync()
            };

            // Recommandations si connecté
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                viewModel.Recommandations = await _livreService.GetRecommandationsAsync(userId);
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
