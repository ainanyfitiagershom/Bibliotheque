using System.ComponentModel.DataAnnotations;

namespace Bibliotheque.Core.DTOs
{
    /// <summary>
    /// DTO pour afficher une catégorie
    /// </summary>
    public class CategorieDTO
    {
        public int IdCategorie { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Couleur { get; set; }
        public int NombreLivres { get; set; }
        public bool Actif { get; set; } = true;
    }

    /// <summary>
    /// DTO pour créer/modifier une catégorie
    /// </summary>
    public class CategorieCreateDTO
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(7)]
        [Display(Name = "Couleur")]
        public string? Couleur { get; set; } = "#6c757d";

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;
    }
}
