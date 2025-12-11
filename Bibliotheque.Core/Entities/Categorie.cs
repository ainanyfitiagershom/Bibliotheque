using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Categories")]
    public class Categorie
    {
        [Key]
        public int IdCategorie { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Format de couleur invalide (ex: #FF5733)")]
        [Display(Name = "Couleur")]
        public string Couleur { get; set; } = "#007bff";

        [StringLength(50)]
        [Display(Name = "Icône")]
        public string Icone { get; set; } = "bi-book";

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        // Navigation
        public virtual ICollection<LivreCategorie> LivreCategories { get; set; } = new List<LivreCategorie>();

        // Propriété calculée
        [NotMapped]
        public int NombreLivres => LivreCategories?.Count ?? 0;
    }
}
