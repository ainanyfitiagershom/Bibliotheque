using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Utilisateurs
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public DetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Utilisateur? Utilisateur { get; set; }
        public List<Emprunt> Emprunts { get; set; } = new();
        public int EmpruntsEnCours { get; set; }
        public int EmpruntsEnRetard { get; set; }
        public int TotalEmprunts { get; set; }
        public int ReservationsActives { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Utilisateur = await _unitOfWork.Utilisateurs.GetByIdAsync(id);
            if (Utilisateur == null) return NotFound();

            // Récupérer les emprunts avec les livres
            var emprunts = await _unitOfWork.Emprunts.GetByUtilisateurAsync(id);
            Emprunts = emprunts.ToList();

            // Calculer les statistiques
            EmpruntsEnCours = Emprunts.Count(e => e.Statut == "EnCours");
            EmpruntsEnRetard = Emprunts.Count(e => e.Statut == "EnRetard");
            TotalEmprunts = Emprunts.Count;

            // Réservations
            var reservations = await _unitOfWork.Reservations.GetByUtilisateurAsync(id);
            ReservationsActives = reservations.Count(r => r.Statut == "EnAttente");

            return Page();
        }
    }
}
