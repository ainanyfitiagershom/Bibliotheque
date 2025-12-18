using Bibliotheque.Core.Entities;
using Bibliotheque.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Backoffice.Razor.Pages.Reservations
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<ReservationViewModel> Reservations { get; set; } = new();
        public string? Statut { get; set; }
        public string? Search { get; set; }

        // Statistiques
        public int TotalEnAttente { get; set; }
        public int TotalDisponible { get; set; }
        public int TotalExpirees { get; set; }

        public async Task OnGetAsync(string? statut = null, string? search = null)
        {
            Statut = statut;
            Search = search;

            var allReservations = await _unitOfWork.Reservations.GetAllAsync();
            var livres = await _unitOfWork.Livres.GetAllAsync();
            var users = await _unitOfWork.Utilisateurs.GetAllAsync();

            // Statistiques
            TotalEnAttente = allReservations.Count(r => r.Statut == "EnAttente");
            TotalDisponible = allReservations.Count(r => r.Statut == "Disponible");
            TotalExpirees = allReservations.Count(r => r.Statut == "Disponible" && r.DateExpiration < DateTime.Now);

            var query = allReservations
                .Where(r => r.Statut == "EnAttente" || r.Statut == "Disponible")
                .AsQueryable();

            if (!string.IsNullOrEmpty(statut))
            {
                query = query.Where(r => r.Statut == statut);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                var livreIds = livres.Where(l => l.Titre.ToLower().Contains(search)).Select(l => l.IdLivre);
                var userIds = users.Where(u => u.Nom.ToLower().Contains(search) ||
                    u.Prenom.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search)).Select(u => u.IdUtilisateur);

                query = query.Where(r => livreIds.Contains(r.IdLivre) || userIds.Contains(r.IdUtilisateur));
            }

            var livresList = livres.ToList();
            var usersList = users.ToList();

            Reservations = query
                .OrderBy(r => r.Statut == "Disponible" ? 0 : 1)
                .ThenBy(r => r.DateReservation)
                .ToList()
                .Select(r =>
                {
                    var livre = livresList.FirstOrDefault(l => l.IdLivre == r.IdLivre);
                    var user = usersList.FirstOrDefault(u => u.IdUtilisateur == r.IdUtilisateur);
                    return new ReservationViewModel
                    {
                        IdReservation = r.IdReservation,
                        IdLivre = r.IdLivre,
                        IdUtilisateur = r.IdUtilisateur,
                        TitreLivre = livre?.Titre ?? "N/A",
                        NomUtilisateur = user != null ? $"{user.Nom} {user.Prenom}" : "N/A",
                        EmailUtilisateur = user?.Email ?? "",
                        DateReservation = r.DateReservation,
                        DateExpiration = r.DateExpiration,
                        Statut = r.Statut,
                        PositionFile = r.PositionFile,
                        EstExpiree = r.Statut == "Disponible" && r.DateExpiration < DateTime.Now,
                        StockDisponible = livre?.StockDisponible ?? 0
                    };
                })
                .ToList();
        }

        public async Task<IActionResult> OnPostNotifierAsync(int id)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
            if (reservation == null)
            {
                TempData["Error"] = "Réservation non trouvée.";
                return RedirectToPage();
            }

            var livre = await _unitOfWork.Livres.GetByIdAsync(reservation.IdLivre);
            if (livre == null || livre.StockDisponible <= 0)
            {
                TempData["Error"] = "Le livre n'est pas disponible en stock. Impossible de notifier.";
                return RedirectToPage();
            }

            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(reservation.IdUtilisateur);

            if (user != null)
            {
                // Créer une notification
                var notification = new Notification
                {
                    IdUtilisateur = user.IdUtilisateur,
                    Type = "Disponibilite",
                    Titre = "Livre disponible",
                    Message = $"Le livre \"{livre.Titre}\" que vous avez réservé est maintenant disponible. Vous avez 3 jours pour venir le récupérer.",
                    DateCreation = DateTime.Now,
                    EstLue = false
                };

                await _unitOfWork.Notifications.AddAsync(notification);

                // Mettre à jour la réservation
                reservation.Statut = "Disponible";
                reservation.DateNotification = DateTime.Now;
                reservation.DateExpiration = DateTime.Now.AddDays(3);

                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"Notification envoyée à {user.Prenom} {user.Nom} pour le livre \"{livre.Titre}\".";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRenotifierAsync(int id)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
            if (reservation == null)
            {
                TempData["Error"] = "Réservation non trouvée.";
                return RedirectToPage();
            }

            var user = await _unitOfWork.Utilisateurs.GetByIdAsync(reservation.IdUtilisateur);
            var livre = await _unitOfWork.Livres.GetByIdAsync(reservation.IdLivre);

            if (user != null && livre != null)
            {
                // Créer une nouvelle notification de rappel
                var notification = new Notification
                {
                    IdUtilisateur = user.IdUtilisateur,
                    Type = "Rappel",
                    Titre = "Rappel - Livre disponible",
                    Message = $"Rappel : Le livre \"{livre.Titre}\" vous attend à la bibliothèque. N'oubliez pas de venir le récupérer !",
                    DateCreation = DateTime.Now,
                    EstLue = false
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                TempData["Success"] = $"Rappel envoyé à {user.Prenom} {user.Nom}.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAnnulerAsync(int id)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
            if (reservation == null)
            {
                TempData["Error"] = "Réservation non trouvée.";
                return RedirectToPage();
            }

            reservation.Statut = "Annulee";
            await _unitOfWork.SaveChangesAsync();

            // Recalculer les positions pour ce livre
            await _unitOfWork.Reservations.RecalculerPositionsFileAsync(reservation.IdLivre);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Réservation annulée.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostConvertirEmpruntAsync(int id)
        {
            var reservation = await _unitOfWork.Reservations.GetByIdAsync(id);
            if (reservation == null)
            {
                TempData["Error"] = "Réservation non trouvée.";
                return RedirectToPage();
            }

            var livre = await _unitOfWork.Livres.GetByIdAsync(reservation.IdLivre);
            if (livre == null || livre.StockDisponible <= 0)
            {
                TempData["Error"] = "Le livre n'est plus disponible.";
                return RedirectToPage();
            }

            // Créer l'emprunt
            var emprunt = new Emprunt
            {
                IdLivre = reservation.IdLivre,
                IdUtilisateur = reservation.IdUtilisateur,
                DateEmprunt = DateTime.Now,
                DateRetourPrevue = DateTime.Now.AddDays(21),
                Statut = "EnCours",
                NombreProlongations = 0
            };

            await _unitOfWork.Emprunts.AddAsync(emprunt);

            // Mettre à jour le stock
            livre.StockDisponible--;

            // Marquer la réservation comme convertie
            reservation.Statut = "Convertie";

            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Réservation convertie en emprunt avec succès.";
            return RedirectToPage();
        }

        public class ReservationViewModel
        {
            public int IdReservation { get; set; }
            public int IdLivre { get; set; }
            public int IdUtilisateur { get; set; }
            public string TitreLivre { get; set; } = string.Empty;
            public string NomUtilisateur { get; set; } = string.Empty;
            public string EmailUtilisateur { get; set; } = string.Empty;
            public DateTime DateReservation { get; set; }
            public DateTime DateExpiration { get; set; }
            public string Statut { get; set; } = "EnAttente";
            public int PositionFile { get; set; }
            public bool EstExpiree { get; set; }
            public int StockDisponible { get; set; }
        }
    }
}
