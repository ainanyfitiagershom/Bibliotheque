using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Emprunts
{
    [Authorize]
    public class RetourModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public RetourModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Emprunt? Emprunt { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(id);
            if (Emprunt == null) return NotFound();

            if (Emprunt.Statut == "Termine")
            {
                TempData["Error"] = "Cet emprunt est déjà terminé.";
                return RedirectToPage("/Emprunts/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id, DateTime dateRetour)
        {
            var emprunt = await _unitOfWork.Emprunts.GetByIdWithDetailsAsync(id);
            if (emprunt == null) return NotFound();

            if (emprunt.Statut == "Termine")
            {
                TempData["Error"] = "Cet emprunt est déjà terminé.";
                return RedirectToPage("/Emprunts/Index");
            }

            // Mettre à jour l'emprunt
            emprunt.DateRetourEffective = dateRetour;
            emprunt.Statut = "Termine";
            await _unitOfWork.Emprunts.UpdateAsync(emprunt);

            // Mettre à jour le stock du livre
            var livre = await _unitOfWork.Livres.GetByIdAsync(emprunt.IdLivre);
            if (livre != null)
            {
                livre.StockDisponible++;
                await _unitOfWork.Livres.UpdateAsync(livre);
            }

            // Vérifier s'il y a des réservations en attente pour ce livre
            var reservations = await _unitOfWork.Reservations.GetByLivreAsync(emprunt.IdLivre);
            var prochainerReservation = reservations
                .Where(r => r.Statut == "EnAttente")
                .OrderBy(r => r.DateReservation)
                .FirstOrDefault();

            if (prochainerReservation != null)
            {
                // Notifier l'utilisateur (marquer la réservation comme disponible)
                prochainerReservation.Statut = "Disponible";
                prochainerReservation.DateExpiration = DateTime.Now.AddDays(3);
                await _unitOfWork.Reservations.UpdateAsync(prochainerReservation);

                // Créer une notification
                var notification = new Notification
                {
                    IdUtilisateur = prochainerReservation.IdUtilisateur,
                    Type = "Disponibilite",
                    Titre = "Livre disponible",
                    Message = $"Le livre \"{livre?.Titre}\" est maintenant disponible. Vous avez 3 jours pour venir le chercher.",
                    DateCreation = DateTime.Now,
                    EstLue = false
                };
                await _unitOfWork.Notifications.AddAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Retour enregistré avec succès.";
            return RedirectToPage("/Emprunts/Index");
        }
    }
}
