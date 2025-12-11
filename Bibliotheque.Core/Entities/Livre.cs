using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Livres")]
    public class Livre
    {
        [Key]
        public int IdLivre { get; set; }

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
        [Display(Name = "Stock total")]
        public int Stock { get; set; } = 1;

        [Range(0, 1000)]
        [Display(Name = "Stock disponible")]
        public int StockDisponible { get; set; } = 1;

        [StringLength(50)]
        [Display(Name = "Emplacement")]
        public string? Emplacement { get; set; }

        [Display(Name = "Date d'ajout")]
        public DateTime DateAjout { get; set; } = DateTime.Now;

        [Display(Name = "Date de modification")]
        public DateTime? DateModification { get; set; }

        [Display(Name = "Nombre d'emprunts")]
        public int NombreEmprunts { get; set; } = 0;

        [Range(0, 5)]
        [Display(Name = "Note moyenne")]
        public decimal NoteMoyenne { get; set; } = 0;

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        // Navigation
        [ForeignKey("IdAuteur")]
        public virtual Auteur? Auteur { get; set; }

        public virtual ICollection<LivreCategorie> LivreCategories { get; set; } = new List<LivreCategorie>();
        public virtual ICollection<Emprunt> Emprunts { get; set; } = new List<Emprunt>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Avis> Avis { get; set; } = new List<Avis>();

        // Propriétés calculées
        [NotMapped]
        public bool EstDisponible => StockDisponible > 0;

        [NotMapped]
        public int StockEmprunte => Stock - StockDisponible;

        [NotMapped]
        public string DisponibiliteTexte => EstDisponible
            ? $"{StockDisponible} disponible(s)"
            : "Indisponible";

        [NotMapped]
        public string CategoriesTexte => LivreCategories != null
            ? string.Join(", ", LivreCategories.Select(lc => lc.Categorie?.Nom))
            : string.Empty;

        [NotMapped]
        public int NombreReservationsEnAttente => Reservations?.Count(r => r.Statut == "EnAttente") ?? 0;
    }
}
