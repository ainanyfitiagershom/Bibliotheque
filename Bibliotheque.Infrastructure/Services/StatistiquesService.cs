using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Interfaces;

namespace Bibliotheque.Infrastructure.Services
{
    public class StatistiquesService : IStatistiquesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatistiquesService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            var stats = new DashboardStatsDTO
            {
                // Compteurs généraux
                TotalLivres = await _unitOfWork.Livres.CountAsync(l => l.Actif),
                TotalAuteurs = await _unitOfWork.Auteurs.CountAsync(a => a.Actif),
                TotalCategories = await _unitOfWork.Categories.CountAsync(c => c.Actif),
                TotalUtilisateurs = await _unitOfWork.Utilisateurs.CountAsync(u => u.Actif),

                // Emprunts
                EmpruntsEnCours = await _unitOfWork.Emprunts.CountAsync(e => e.Statut == "EnCours"),
                EmpruntsEnRetard = await _unitOfWork.Emprunts.CountAsync(e =>
                    e.Statut == "EnRetard" || (e.Statut == "EnCours" && e.DateRetourPrevue < DateTime.Now)),
                ReservationsEnAttente = await _unitOfWork.Reservations.CountAsync(r => r.Statut == "EnAttente")
            };

            // Calcul du total d'exemplaires
            var livres = await _unitOfWork.Livres.FindAsync(l => l.Actif);
            stats.TotalExemplaires = livres.Sum(l => l.Stock);

            // Emprunts et retours du jour
            var aujourdhui = DateTime.Today;
            var demain = aujourdhui.AddDays(1);
            stats.EmpruntsAujourdhui = await _unitOfWork.Emprunts.CountAsync(e =>
                e.DateEmprunt >= aujourdhui && e.DateEmprunt < demain);
            stats.RetoursAujourdhui = await _unitOfWork.Emprunts.CountAsync(e =>
                e.DateRetourEffective != null &&
                e.DateRetourEffective >= aujourdhui && e.DateRetourEffective < demain);

            // Top livres
            stats.TopLivres = (await GetTopLivresAsync(10)).ToList();

            // Emprunts par mois
            stats.EmpruntsParMois = (await GetEmpruntsParMoisAsync(12)).ToList();

            // Emprunts par catégorie
            stats.EmpruntsParCategorie = (await GetEmpruntsParCategorieAsync()).ToList();

            return stats;
        }

        public async Task<IEnumerable<TopLivreDTO>> GetTopLivresAsync(int nombre = 10)
        {
            var livres = await _unitOfWork.Livres.GetPopulairesAsync(nombre);

            return livres.Select(l => new TopLivreDTO
            {
                IdLivre = l.IdLivre,
                Titre = l.Titre,
                Auteur = l.Auteur?.NomComplet ?? "Inconnu",
                NombreEmprunts = l.NombreEmprunts,
                ImageCouverture = l.ImageCouverture
            });
        }

        public async Task<IEnumerable<EmpruntParMoisDTO>> GetEmpruntsParMoisAsync(int nombreMois = 12)
        {
            return await _unitOfWork.Emprunts.GetStatistiquesParMoisAsync(nombreMois);
        }

        public async Task<IEnumerable<EmpruntParCategorieDTO>> GetEmpruntsParCategorieAsync()
        {
            return await _unitOfWork.Emprunts.GetStatistiquesParCategorieAsync();
        }

        public async Task<RapportActiviteDTO> GetRapportActiviteAsync(DateTime dateDebut, DateTime dateFin)
        {
            var rapport = new RapportActiviteDTO
            {
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            // Nouveaux utilisateurs
            rapport.NouveauxUtilisateurs = await _unitOfWork.Utilisateurs.CountAsync(u =>
                u.DateInscription >= dateDebut && u.DateInscription <= dateFin);

            // Nouveaux livres
            rapport.NouveauxLivres = await _unitOfWork.Livres.CountAsync(l =>
                l.DateAjout >= dateDebut && l.DateAjout <= dateFin);

            // Emprunts de la période
            var emprunts = await _unitOfWork.Emprunts.FindAsync(e =>
                e.DateEmprunt >= dateDebut && e.DateEmprunt <= dateFin);
            rapport.TotalEmprunts = emprunts.Count();

            // Retours de la période
            rapport.TotalRetours = await _unitOfWork.Emprunts.CountAsync(e =>
                e.DateRetourEffective != null &&
                e.DateRetourEffective >= dateDebut && e.DateRetourEffective <= dateFin);

            // Retards
            rapport.TotalRetards = await _unitOfWork.Emprunts.CountAsync(e =>
                (e.Statut == "EnRetard" || (e.Statut == "Termine" && e.Penalite > 0)) &&
                e.DateEmprunt >= dateDebut && e.DateEmprunt <= dateFin);

            // Total des pénalités
            var empruntsAvecPenalite = await _unitOfWork.Emprunts.FindAsync(e =>
                e.Penalite > 0 &&
                e.DateRetourEffective >= dateDebut && e.DateRetourEffective <= dateFin);
            rapport.TotalPenalites = empruntsAvecPenalite.Sum(e => e.Penalite);

            // Top livres de la période
            rapport.LivresLesPlusEmpruntes = (await GetTopLivresAsync(5)).ToList();

            // Utilisateurs les plus actifs
            var utilisateursActifs = await _unitOfWork.Utilisateurs.GetPlusActifsAsync(5);
            rapport.UtilisateursLesPlusActifs = utilisateursActifs.Select(u => new UtilisateurActifDTO
            {
                IdUtilisateur = u.IdUtilisateur,
                NomComplet = u.NomComplet,
                NombreEmprunts = u.Emprunts.Count(e => e.DateEmprunt >= dateDebut && e.DateEmprunt <= dateFin)
            }).ToList();

            return rapport;
        }
    }
}
