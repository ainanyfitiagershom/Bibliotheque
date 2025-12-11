using System.ComponentModel.DataAnnotations;

namespace Bibliotheque.Core.DTOs
{
    /// <summary>
    /// DTO pour afficher un emprunt
    /// </summary>
    public class EmpruntDTO
    {
        public int IdEmprunt { get; set; }
        public int IdLivre { get; set; }
        public string LivreTitre { get; set; } = string.Empty;
        public string? LivreImage { get; set; }
        public int IdUtilisateur { get; set; }
        public string UtilisateurNom { get; set; } = string.Empty;
        public string UtilisateurEmail { get; set; } = string.Empty;
        public DateTime DateEmprunt { get; set; }
        public DateTime DateRetourPrevue { get; set; }
        public DateTime? DateRetourEffective { get; set; }
        public string Statut { get; set; } = string.Empty;
        public int NombreProlongations { get; set; }
        public int MaxProlongations { get; set; }
        public decimal Penalite { get; set; }
        public string? Notes { get; set; }

        // Propriétés alias pour compatibilité avec les vues
        public string TitreLivre { get => LivreTitre; set => LivreTitre = value; }
        public string? NomAuteur { get; set; }
        public string? ImageCouverture { get => LivreImage; set => LivreImage = value; }

        // Propriétés calculées
        public bool EstEnRetard => Statut == "EnCours" && DateTime.Now > DateRetourPrevue;
        public int JoursRestants => Statut == "Termine" ? 0 : (DateRetourPrevue.Date - DateTime.Today).Days;
        public int JoursRetard => EstEnRetard ? (DateTime.Today - DateRetourPrevue.Date).Days : 0;
        public bool PeutProlonger => Statut == "EnCours" && NombreProlongations < MaxProlongations;

        public string StatutAffichage => Statut switch
        {
            "EnCours" when EstEnRetard => "En retard",
            "EnCours" => "En cours",
            "Termine" => "Terminé",
            "EnRetard" => "En retard",
            _ => Statut
        };

        public string StatutCouleur => Statut switch
        {
            "Termine" => "success",
            "EnRetard" => "danger",
            "EnCours" when EstEnRetard => "danger",
            "EnCours" when JoursRestants <= 2 => "warning",
            _ => "primary"
        };
    }

    /// <summary>
    /// DTO pour créer un emprunt
    /// </summary>
    public class EmpruntCreateDTO
    {
        [Required(ErrorMessage = "Le livre est obligatoire")]
        [Display(Name = "Livre")]
        public int IdLivre { get; set; }

        [Required(ErrorMessage = "L'utilisateur est obligatoire")]
        [Display(Name = "Utilisateur")]
        public int IdUtilisateur { get; set; }

        [Range(1, 60, ErrorMessage = "La durée doit être entre 1 et 60 jours")]
        [Display(Name = "Durée (jours)")]
        public int DureeJours { get; set; } = 14;

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO pour effectuer un retour
    /// </summary>
    public class RetourDTO
    {
        [Required]
        public int IdEmprunt { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO pour prolonger un emprunt
    /// </summary>
    public class ProlongationDTO
    {
        [Required]
        public int IdEmprunt { get; set; }

        [Range(1, 30, ErrorMessage = "La prolongation doit être entre 1 et 30 jours")]
        [Display(Name = "Nombre de jours")]
        public int NombreJours { get; set; } = 7;
    }

    /// <summary>
    /// DTO pour le filtre des emprunts
    /// </summary>
    public class EmpruntFiltreDTO
    {
        public int? IdUtilisateur { get; set; }
        public int? IdLivre { get; set; }
        public string? Statut { get; set; } // EnCours, Termine, EnRetard, Tous
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool? EnRetard { get; set; }
        public int Page { get; set; } = 1;
        public int TaillePage { get; set; } = 10;
    }

    /// <summary>
    /// DTO pour créer une réservation
    /// </summary>
    public class ReservationCreateDTO
    {
        [Required(ErrorMessage = "Le livre est obligatoire")]
        [Display(Name = "Livre")]
        public int IdLivre { get; set; }

        [Required(ErrorMessage = "L'utilisateur est obligatoire")]
        [Display(Name = "Utilisateur")]
        public int IdUtilisateur { get; set; }
    }

    /// <summary>
    /// DTO pour afficher une réservation
    /// </summary>
    public class ReservationDTO
    {
        public int IdReservation { get; set; }
        public int IdLivre { get; set; }
        public string LivreTitre { get; set; } = string.Empty;
        public string? LivreImage { get; set; }
        public int IdUtilisateur { get; set; }
        public string UtilisateurNom { get; set; } = string.Empty;
        public DateTime DateReservation { get; set; }
        public DateTime? DateExpiration { get; set; }
        public int PositionFile { get; set; }
        public string Statut { get; set; } = string.Empty;

        // Propriétés alias pour compatibilité avec les vues
        public string TitreLivre { get => LivreTitre; set => LivreTitre = value; }
        public string? NomAuteur { get; set; }
        public string? ImageCouverture { get => LivreImage; set => LivreImage = value; }

        public string StatutAffichage => Statut switch
        {
            "EnAttente" => $"En attente (position {PositionFile})",
            "Disponible" => "Disponible - À récupérer",
            "Annulee" => "Annulée",
            "Convertie" => "Convertie en emprunt",
            _ => Statut
        };

        public string StatutCouleur => Statut switch
        {
            "EnAttente" => "warning",
            "Disponible" => "success",
            "Annulee" => "secondary",
            "Convertie" => "info",
            _ => "primary"
        };
    }
}
