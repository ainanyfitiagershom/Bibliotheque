using Frontoffice.MVC.Models;
using Frontoffice.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontoffice.MVC.Controllers
{
    public class LivresController : Controller
    {
        private readonly ILivreService _livreService;
        private readonly IReservationService _reservationService;

        public LivresController(ILivreService livreService, IReservationService reservationService)
        {
            _livreService = livreService;
            _reservationService = reservationService;
        }

        public async Task<IActionResult> Index(string? search, int? categorieId, int page = 1, string tri = "titre")
        {
            var result = await _livreService.RechercherAsync(search, categorieId, page, 12, tri);
            var categories = await _livreService.GetCategoriesAsync();

            var viewModel = new LivresIndexViewModel
            {
                Livres = result,
                Categories = categories,
                Search = search,
                CategorieId = categorieId,
                Tri = tri
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var livre = await _livreService.GetByIdAsync(id);
            if (livre == null) return NotFound();

            var viewModel = new LivreDetailsViewModel
            {
                Livre = livre,
                PositionFileAttente = await _reservationService.GetPositionFileAttenteAsync(id)
            };

            // Vérifier si l'utilisateur connecté a déjà réservé ce livre
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId > 0)
                {
                    viewModel.DejaReserve = await _reservationService.ADejaReserveAsync(id, userId);
                }
            }

            return View(viewModel);
        }

        // API pour la recherche AJAX
        [HttpGet]
        public async Task<IActionResult> Rechercher(string? q, int? categorieId, int page = 1)
        {
            var result = await _livreService.RechercherAsync(q, categorieId, page, 12, "titre");
            return Json(result);
        }
    }
}
