using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Emprunts")]
    public class Emprunt
    {
        [Key]
        public int IdEmprunt { get; set; }

        [Required(ErrorMessage = "Le livre est obligatoire")]
        [Display(Name = "Livre")]
        public int IdLivre { get; set; }

        [Required(ErrorMessage = "L'utilisateur est obligatoire")]
        [Display(Name = "Utilisateur")]
        public int IdUtilisateur { get; set; }

        [Display(Name = "Date d'emprunt")]
        public DateTime DateEmprunt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La date de retour prévue est obligatoire")]
        [Display(Name = "Date de retour prévue")]
        public DateTime DateRetourPrevue { get; set; }

        [Display(Name = "Date de retour effective")]
        public DateTime? DateRetourEffective { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Statut")]
        public string Statut { get; set; } = "EnCours"; // EnCours, Termine, EnRetard

        [Range(0, 10)]
        [Display(Name = "Nombre de prolongations")]
        public int NombreProlongations { get; set; } = 0;

        [Range(0, 10)]
        [Display(Name = "Max prolongations")]
        public int MaxProlongations { get; set; } = 2;

        [Range(0, 1000)]
        [Display(Name = "Pénalité (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Penalite { get; set; } = 0;

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Navigation
        [ForeignKey("IdLivre")]
        public virtual Livre? Livre { get; set; }

        [ForeignKey("IdUtilisateur")]
        public virtual Utilisateur? Utilisateur { get; set; }

        // Propriétés calculées
        [NotMapped]
        public bool EstEnRetard => Statut == "EnCours" && DateTime.Now > DateRetourPrevue;

        [NotMapped]
        public int JoursRestants
        {
            get
            {
                if (Statut == "Termine") return 0;
                return (DateRetourPrevue.Date - DateTime.Today).Days;
            }
        }

        [NotMapped]
        public int JoursRetard
        {
            get
            {
                if (!EstEnRetard && Statut != "EnRetard") return 0;
                var dateRef = DateRetourEffective ?? DateTime.Now;
                return Math.Max(0, (dateRef.Date - DateRetourPrevue.Date).Days);
            }
        }

        [NotMapped]
        public int DureeEmprunt
        {
            get
            {
                var dateFin = DateRetourEffective ?? DateTime.Now;
                return (dateFin.Date - DateEmprunt.Date).Days;
            }
        }

        [NotMapped]
        public bool PeutProlonger => Statut == "EnCours" && NombreProlongations < MaxProlongations;

        [NotMapped]
        public string StatutAffichage
        {
            get
            {
                return Statut switch
                {
                    "EnCours" when EstEnRetard => "En retard",
                    "EnCours" => "En cours",
                    "Termine" => "Terminé",
                    "EnRetard" => "En retard",
                    _ => Statut
                };
            }
        }

        [NotMapped]
        public string StatutCouleur
        {
            get
            {
                return Statut switch
                {
                    "Termine" => "success",
                    "EnRetard" => "danger",
                    "EnCours" when EstEnRetard => "danger",
                    "EnCours" when JoursRestants <= 2 => "warning",
                    _ => "primary"
                };
            }
        }
    }
}
