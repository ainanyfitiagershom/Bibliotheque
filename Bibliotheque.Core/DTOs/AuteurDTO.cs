using System.ComponentModel.DataAnnotations;

namespace Bibliotheque.Core.DTOs
{
    /// <summary>
    /// DTO pour afficher un auteur
    /// </summary>
    public class AuteurDTO
    {
        public int IdAuteur { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Prenom { get; set; }
        public string? Biographie { get; set; }
        public string? Photo { get; set; }
        public DateTime? DateNaissance { get; set; }
        public DateTime? DateDeces { get; set; }
        public string? Nationalite { get; set; }
        public int NombreLivres { get; set; }
        public bool Actif { get; set; } = true;

        // Alias pour compatibilité avec les vues
        public string? PhotoUrl { get => Photo; set => Photo = value; }

        // Propriétés calculées
        public string NomComplet => string.IsNullOrEmpty(Prenom) ? Nom : $"{Prenom} {Nom}";
    }

    /// <summary>
    /// DTO pour créer/modifier un auteur
    /// </summary>
    public class AuteurCreateDTO
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Prénom")]
        public string? Prenom { get; set; }

        [Display(Name = "Biographie")]
        public string? Biographie { get; set; }

        [StringLength(500)]
        [Display(Name = "Photo (URL)")]
        public string? Photo { get; set; }

        [Display(Name = "Date de naissance")]
        [DataType(DataType.Date)]
        public DateTime? DateNaissance { get; set; }

        [StringLength(50)]
        [Display(Name = "Nationalité")]
        public string? Nationalite { get; set; }

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;
    }
}
