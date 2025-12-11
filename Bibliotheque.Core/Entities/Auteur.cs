using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Auteurs")]
    public class Auteur
    {
        [Key]
        public int IdAuteur { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Prénom")]
        public string? Prenom { get; set; }

        [StringLength(100)]
        [Display(Name = "Nationalité")]
        public string? Nationalite { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de naissance")]
        public DateTime? DateNaissance { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de décès")]
        public DateTime? DateDeces { get; set; }

        [Display(Name = "Biographie")]
        public string? Biographie { get; set; }

        [Url(ErrorMessage = "URL invalide")]
        [StringLength(500)]
        [Display(Name = "Photo")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        // Navigation
        public virtual ICollection<Livre> Livres { get; set; } = new List<Livre>();

        // Propriétés calculées
        [NotMapped]
        public string NomComplet => string.IsNullOrEmpty(Prenom) ? Nom : $"{Prenom} {Nom}";

        [NotMapped]
        public int NombreLivres => Livres?.Count(l => l.Actif) ?? 0;

        [NotMapped]
        public bool EstVivant => !DateDeces.HasValue;

        [NotMapped]
        public int? Age
        {
            get
            {
                if (!DateNaissance.HasValue) return null;
                var endDate = DateDeces ?? DateTime.Today;
                var age = endDate.Year - DateNaissance.Value.Year;
                if (endDate < DateNaissance.Value.AddYears(age)) age--;
                return age;
            }
        }
    }
}
