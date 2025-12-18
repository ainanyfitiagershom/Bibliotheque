using Frontoffice.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontoffice.MVC.Controllers
{
    [Authorize]
    public class EmpruntsController : Controller
    {
        private readonly IEmpruntService _empruntService;

        public EmpruntsController(IEmpruntService empruntService)
        {
            _empruntService = empruntService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var emprunts = await _empruntService.GetEmpruntsUtilisateurAsync(userId);
            return View(emprunts);
        }

        [HttpPost]
        public async Task<IActionResult> Prolonger(int id)
        {
            var userId = GetUserId();
            var (success, errorMessage) = await _empruntService.ProlongerEmpruntAsync(id, userId);

            if (success)
                TempData["Success"] = "Emprunt prolongé de 7 jours avec succès.";
            else
                TempData["Error"] = errorMessage ?? "Impossible de prolonger cet emprunt.";

            return RedirectToAction(nameof(Index));
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}
