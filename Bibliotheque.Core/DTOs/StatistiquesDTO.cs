namespace Bibliotheque.Core.DTOs
{
    /// <summary>
    /// DTO pour les statistiques du dashboard
    /// </summary>
    public class DashboardStatsDTO
    {
        // Compteurs généraux
        public int TotalLivres { get; set; }
        public int TotalExemplaires { get; set; }
        public int TotalUtilisateurs { get; set; }
        public int TotalAuteurs { get; set; }
        public int TotalCategories { get; set; }

        // Emprunts
        public int EmpruntsEnCours { get; set; }
        public int EmpruntsEnRetard { get; set; }
        public int ReservationsEnAttente { get; set; }
        public int EmpruntsAujourdhui { get; set; }
        public int RetoursAujourdhui { get; set; }

        // Données pour graphiques
        public List<TopLivreDTO> TopLivres { get; set; } = new();
        public List<EmpruntParMoisDTO> EmpruntsParMois { get; set; } = new();
        public List<EmpruntParCategorieDTO> EmpruntsParCategorie { get; set; } = new();

        // Indicateurs
        public decimal TauxOccupation => TotalExemplaires > 0
            ? Math.Round((decimal)(TotalExemplaires - EmpruntsEnCours) / TotalExemplaires * 100, 1)
            : 0;

        public decimal TauxRetard => EmpruntsEnCours > 0
            ? Math.Round((decimal)EmpruntsEnRetard / EmpruntsEnCours * 100, 1)
            : 0;
    }

    /// <summary>
    /// DTO pour les livres les plus populaires
    /// </summary>
    public class TopLivreDTO
    {
        public int IdLivre { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Auteur { get; set; } = string.Empty;
        public int NombreEmprunts { get; set; }
        public string? ImageCouverture { get; set; }
    }

    /// <summary>
    /// DTO pour les emprunts par mois
    /// </summary>
    public class EmpruntParMoisDTO
    {
        public string Mois { get; set; } = string.Empty; // Format: "2024-01"
        public int NombreEmprunts { get; set; }

        public string MoisFormate
        {
            get
            {
                if (DateTime.TryParse(Mois + "-01", out var date))
                    return date.ToString("MMM yyyy");
                return Mois;
            }
        }
    }

    /// <summary>
    /// DTO pour les emprunts par catégorie
    /// </summary>
    public class EmpruntParCategorieDTO
    {
        public string Categorie { get; set; } = string.Empty;
        public int NombreEmprunts { get; set; }
        public string? Couleur { get; set; }
    }

    /// <summary>
    /// DTO pour les alertes du dashboard
    /// </summary>
    public class AlerteDTO
    {
        public string Type { get; set; } = string.Empty; // danger, warning, info
        public string Titre { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Lien { get; set; }
        public int Nombre { get; set; }
    }

    /// <summary>
    /// DTO pour l'activité récente
    /// </summary>
    public class ActiviteRecenteDTO
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // Emprunt, Retour, Inscription, etc.
        public string Description { get; set; } = string.Empty;
        public string? Lien { get; set; }
        public string Icone { get; set; } = "bi-activity";
    }

    /// <summary>
    /// DTO pour le rapport d'activité
    /// </summary>
    public class RapportActiviteDTO
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int NouveauxUtilisateurs { get; set; }
        public int NouveauxLivres { get; set; }
        public int TotalEmprunts { get; set; }
        public int TotalRetours { get; set; }
        public int TotalRetards { get; set; }
        public decimal TotalPenalites { get; set; }
        public List<TopLivreDTO> LivresLesPlusEmpruntes { get; set; } = new();
        public List<UtilisateurActifDTO> UtilisateursLesPlusActifs { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les utilisateurs les plus actifs
    /// </summary>
    public class UtilisateurActifDTO
    {
        public int IdUtilisateur { get; set; }
        public string NomComplet { get; set; } = string.Empty;
        public int NombreEmprunts { get; set; }
    }
}
