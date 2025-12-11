using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Emprunts
{
    [Authorize]
    public class ProlongerModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProlongerModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Emprunt? Emprunt { get; set; }
        public bool HasReservation { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(id);
            if (Emprunt == null) return NotFound();

            if (Emprunt.Statut == "Termine")
            {
                TempData["Error"] = "Cet emprunt est déjà terminé.";
                return RedirectToPage("/Emprunts/Index");
            }

            // Vérifier s'il y a des réservations
            var reservations = await _unitOfWork.Reservations.GetByLivreAsync(Emprunt.IdLivre);
            HasReservation = reservations.Any(r => r.Statut == "EnAttente");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(id);
            if (emprunt == null) return NotFound();

            if (emprunt.Statut == "Termine")
            {
                TempData["Error"] = "Cet emprunt est déjà terminé.";
                return RedirectToPage("/Emprunts/Index");
            }

            if (emprunt.NombreProlongations >= 2)
            {
                TempData["Error"] = "Nombre maximum de prolongations atteint.";
                return RedirectToPage("/Emprunts/Index");
            }

            // Vérifier les réservations
            var reservations = await _unitOfWork.Reservations.GetByLivreAsync(emprunt.IdLivre);
            if (reservations.Any(r => r.Statut == "EnAttente"))
            {
                TempData["Error"] = "Ce livre a des réservations en attente.";
                return RedirectToPage("/Emprunts/Index");
            }

            // Prolonger
            emprunt.DateRetourPrevue = emprunt.DateRetourPrevue.AddDays(7);
            emprunt.NombreProlongations++;

            // Si l'emprunt était en retard et que la nouvelle date est dans le futur
            if (emprunt.Statut == "EnRetard" && emprunt.DateRetourPrevue > DateTime.Today)
            {
                emprunt.Statut = "EnCours";
            }

            await _unitOfWork.Emprunts.UpdateAsync(emprunt);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = $"Emprunt prolongé jusqu'au {emprunt.DateRetourPrevue:dd/MM/yyyy}.";
            return RedirectToPage("/Emprunts/Index");
        }
    }
}
