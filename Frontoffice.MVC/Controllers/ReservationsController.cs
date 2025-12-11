using Frontoffice.MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Frontoffice.MVC.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var reservations = await _reservationService.GetReservationsUtilisateurAsync(userId);
            return View(reservations);
        }

        // Si quelqu'un accède à /Reservations/Reserver via GET, rediriger vers le catalogue
        [HttpGet]
        public IActionResult Reserver()
        {
            TempData["Error"] = "Pour réserver un livre, veuillez d'abord sélectionner un livre depuis le catalogue.";
            return RedirectToAction("Index", "Livres");
        }

        [HttpPost]
        public async Task<IActionResult> Reserver(int livreId)
        {
            var userId = GetUserId();
            var success = await _reservationService.ReserverAsync(livreId, userId);

            if (success)
                TempData["Success"] = "Réservation effectuée avec succès.";
            else
                TempData["Error"] = "Impossible de réserver ce livre. Vous l'avez peut-être déjà réservé ou emprunté.";

            return RedirectToAction("Details", "Livres", new { id = livreId });
        }

        [HttpPost]
        public async Task<IActionResult> Annuler(int id)
        {
            var userId = GetUserId();
            var success = await _reservationService.AnnulerReservationAsync(id, userId);

            if (success)
                TempData["Success"] = "Réservation annulée.";
            else
                TempData["Error"] = "Impossible d'annuler cette réservation.";

            return RedirectToAction(nameof(Index));
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}
