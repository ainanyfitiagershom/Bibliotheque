using System.ComponentModel.DataAnnotations;

namespace Bibliotheque.Core.DTOs
{
    /// <summary>
    /// DTO pour afficher un livre (lecture)
    /// </summary>
    public class LivreDTO
    {
        public int IdLivre { get; set; }
        public string? ISBN { get; set; }
        public string Titre { get; set; } = string.Empty;
        public int? IdAuteur { get; set; }
        public string? NomAuteur { get; set; }
        public string AuteurNom { get => NomAuteur ?? string.Empty; set => NomAuteur = value; }
        public int? Annee { get; set; }
        public string? Editeur { get; set; }
        public int? NombrePages { get; set; }
        public string? Langue { get; set; } = "Français";
        public string? Description { get; set; }
        public string? ImageCouverture { get; set; }
        public int Stock { get; set; }
        public int StockDisponible { get; set; }
        public string? Emplacement { get; set; }
        public int NombreEmprunts { get; set; }
        public decimal NoteMoyenne { get; set; }
        public int NombreAvis { get; set; }
        public List<CategorieSimpleDTO>? Categories { get; set; }
        public bool EstDisponible => StockDisponible > 0;
    }

    public class CategorieSimpleDTO
    {
        public int IdCategorie { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Couleur { get; set; }
    }

    /// <summary>
    /// DTO pour créer/modifier un livre
    /// </summary>
    public class LivreCreateDTO
    {
        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(255, ErrorMessage = "Le titre ne peut pas dépasser 255 caractères")]
        [Display(Name = "Titre")]
        public string Titre { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'auteur est obligatoire")]
        [Display(Name = "Auteur")]
        public int IdAuteur { get; set; }

        [Range(1000, 2100, ErrorMessage = "L'année doit être comprise entre 1000 et 2100")]
        [Display(Name = "Année de publication")]
        public int? Annee { get; set; }

        [StringLength(200)]
        [Display(Name = "Éditeur")]
        public string? Editeur { get; set; }

        [Range(1, 10000, ErrorMessage = "Le nombre de pages doit être entre 1 et 10000")]
        [Display(Name = "Nombre de pages")]
        public int? NombrePages { get; set; }

        [StringLength(50)]
        [Display(Name = "Langue")]
        public string Langue { get; set; } = "Français";

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "URL invalide")]
        [StringLength(500)]
        [Display(Name = "Image de couverture")]
        public string? ImageCouverture { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Le stock doit être entre 0 et 1000")]
        [Display(Name = "Stock")]
        public int Stock { get; set; } = 1;

        [StringLength(50)]
        [Display(Name = "Emplacement")]
        public string? Emplacement { get; set; }

        [Display(Name = "Catégories")]
        public List<int> CategorieIds { get; set; } = new();
    }

    /// <summary>
    /// DTO pour la recherche de livres
    /// </summary>
    public class LivreRechercheDTO
    {
        public string? Recherche { get; set; }
        public int? IdCategorie { get; set; }
        public int? IdAuteur { get; set; }
        public int? Annee { get; set; }
        public bool? Disponible { get; set; }
        public string Tri { get; set; } = "Titre";
        public int Page { get; set; } = 1;
        public int TaillePage { get; set; } = 10;
    }

    /// <summary>
    /// DTO pour les résultats paginés
    /// </summary>
    public class PagedResultDTO<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalCount { get => TotalItems; set => TotalItems = value; }
        public int Page { get; set; }
        public int PageSize { get => TaillePage; set => TaillePage = value; }
        public int TaillePage { get; set; } = 10;
        public int TotalPages => TaillePage > 0 ? (int)Math.Ceiling((double)TotalItems / TaillePage) : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// DTO pour l'import CSV de livres
    /// </summary>
    public class LivreImportDTO
    {
        public string? ISBN { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string AuteurNom { get; set; } = string.Empty;
        public string? AuteurPrenom { get; set; }
        public int? Annee { get; set; }
        public string? Editeur { get; set; }
        public int? NombrePages { get; set; }
        public string Langue { get; set; } = "Français";
        public string? Description { get; set; }
        public int Stock { get; set; } = 1;
        public string? Categories { get; set; } // Séparées par des virgules
    }

    /// <summary>
    /// DTO pour le résultat d'import
    /// </summary>
    public class ImportResultDTO
    {
        public int TotalLignes { get; set; }
        public int LignesImportees { get; set; }
        public int LignesErreur { get; set; }
        public List<string> Erreurs { get; set; } = new();
        public bool Succes => LignesErreur == 0;
    }
}
