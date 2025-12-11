using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int IdNotification { get; set; }

        [Required]
        [Display(Name = "Utilisateur")]
        public int IdUtilisateur { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string Type { get; set; } = "Systeme"; // Retard, Disponibilite, Rappel, Systeme, Bienvenue

        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(200)]
        [Display(Name = "Titre")]
        public string Titre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le message est obligatoire")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Date de lecture")]
        public DateTime? DateLecture { get; set; }

        [Display(Name = "Lue")]
        public bool EstLue { get; set; } = false;

        // Alias pour compatibilité avec les vues
        [NotMapped]
        public bool Lu { get => EstLue; set => EstLue = value; }

        [StringLength(500)]
        [Display(Name = "Lien")]
        public string? Lien { get; set; }

        // Navigation
        [ForeignKey("IdUtilisateur")]
        public virtual Utilisateur? Utilisateur { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string TypeIcone
        {
            get
            {
                return Type switch
                {
                    "Retard" => "bi-exclamation-triangle-fill text-danger",
                    "Disponibilite" => "bi-check-circle-fill text-success",
                    "Rappel" => "bi-bell-fill text-warning",
                    "Bienvenue" => "bi-hand-thumbs-up-fill text-primary",
                    _ => "bi-info-circle-fill text-info"
                };
            }
        }

        [NotMapped]
        public string TypeCouleur
        {
            get
            {
                return Type switch
                {
                    "Retard" => "danger",
                    "Disponibilite" => "success",
                    "Rappel" => "warning",
                    "Bienvenue" => "primary",
                    _ => "info"
                };
            }
        }

        [NotMapped]
        public string TempsEcoule
        {
            get
            {
                var diff = DateTime.Now - DateCreation;
                if (diff.TotalMinutes < 1) return "À l'instant";
                if (diff.TotalMinutes < 60) return $"Il y a {(int)diff.TotalMinutes} min";
                if (diff.TotalHours < 24) return $"Il y a {(int)diff.TotalHours}h";
                if (diff.TotalDays < 7) return $"Il y a {(int)diff.TotalDays} jour(s)";
                return DateCreation.ToString("dd/MM/yyyy");
            }
        }
    }
}
