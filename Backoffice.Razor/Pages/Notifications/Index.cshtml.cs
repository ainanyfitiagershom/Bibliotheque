using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmpruntService _empruntService;
        private const int PageSize = 20;

        public IndexModel(IUnitOfWork unitOfWork, IEmpruntService empruntService)
        {
            _unitOfWork = unitOfWork;
            _empruntService = empruntService;
        }

        public List<NotificationViewModel> Notifications { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public string? TypeFiltre { get; set; }
        public string? Search { get; set; }

        // Statistiques
        public int TotalNotifications { get; set; }
        public int TotalRetard { get; set; }
        public int TotalDisponibilite { get; set; }
        public int TotalRappel { get; set; }
        public int TotalNonLues { get; set; }

        public async Task OnGetAsync(int page = 1, string? type = null, string? search = null)
        {
            CurrentPage = page;
            TypeFiltre = type;
            Search = search;

            var allNotifications = await _unitOfWork.Notifications.GetAllAsync();
            var users = await _unitOfWork.Utilisateurs.GetAllAsync();

            // Statistiques
            TotalNotifications = allNotifications.Count();
            TotalRetard = allNotifications.Count(n => n.Type == "Retard");
            TotalDisponibilite = allNotifications.Count(n => n.Type == "Disponibilite");
            TotalRappel = allNotifications.Count(n => n.Type == "Rappel");
            TotalNonLues = allNotifications.Count(n => !n.EstLue);

            var query = allNotifications.AsQueryable();

            // Filtre par type
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(n => n.Type == type);
            }

            // Recherche
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                var userIds = users.Where(u =>
                    u.Nom.ToLower().Contains(search) ||
                    u.Prenom.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search))
                    .Select(u => u.IdUtilisateur);

                query = query.Where(n =>
                    n.Titre.ToLower().Contains(search) ||
                    n.Message.ToLower().Contains(search) ||
                    userIds.Contains(n.IdUtilisateur));
            }

            var total = query.Count();
            TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            var notifList = query
                .OrderByDescending(n => n.IdNotification)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            Notifications = notifList.Select(n =>
            {
                var user = users.FirstOrDefault(u => u.IdUtilisateur == n.IdUtilisateur);
                return new NotificationViewModel
                {
                    IdNotification = n.IdNotification,
                    IdUtilisateur = n.IdUtilisateur,
                    NomUtilisateur = user != null ? $"{user.Nom} {user.Prenom}" : "Utilisateur inconnu",
                    Type = n.Type,
                    Titre = n.Titre,
                    Message = n.Message,
                    DateCreation = n.DateCreation,
                    EstLue = n.EstLue,
                    DateLecture = n.DateLecture,
                    Lien = n.Lien,
                    TypeIcone = n.TypeIcone,
                    TypeCouleur = n.TypeCouleur,
                    TempsEcoule = n.TempsEcoule
                };
            }).ToList();
        }

        public async Task<IActionResult> OnPostDetecterRetardsAsync()
        {
            try
            {
                await _empruntService.DetecterRetardsAsync();
                TempData["Success"] = "Détection des retards effectuée avec succès. Les notifications ont été envoyées.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la détection des retards : {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSupprimerAnciennesAsync()
        {
            try
            {
                await _unitOfWork.Notifications.SupprimerAnciennesAsync(30);
                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Les notifications de plus de 30 jours ont été supprimées.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSupprimerAsync(int id)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
                if (notification != null)
                {
                    await _unitOfWork.Notifications.DeleteAsync(notification);
                    await _unitOfWork.SaveChangesAsync();
                    TempData["Success"] = "Notification supprimée avec succès.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors de la suppression : {ex.Message}";
            }

            return RedirectToPage();
        }

        public class NotificationViewModel
        {
            public int IdNotification { get; set; }
            public int IdUtilisateur { get; set; }
            public string NomUtilisateur { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Titre { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime DateCreation { get; set; }
            public bool EstLue { get; set; }
            public DateTime? DateLecture { get; set; }
            public string? Lien { get; set; }
            public string TypeIcone { get; set; } = string.Empty;
            public string TypeCouleur { get; set; } = string.Empty;
            public string TempsEcoule { get; set; } = string.Empty;
        }
    }
}
