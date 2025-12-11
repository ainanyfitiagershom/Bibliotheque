using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        public int IdReservation { get; set; }

        [Required(ErrorMessage = "Le livre est obligatoire")]
        [Display(Name = "Livre")]
        public int IdLivre { get; set; }

        [Required(ErrorMessage = "L'utilisateur est obligatoire")]
        [Display(Name = "Utilisateur")]
        public int IdUtilisateur { get; set; }

        [Display(Name = "Date de réservation")]
        public DateTime DateReservation { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La date d'expiration est obligatoire")]
        [Display(Name = "Date d'expiration")]
        public DateTime DateExpiration { get; set; }

        [Range(1, 100)]
        [Display(Name = "Position dans la file")]
        public int PositionFile { get; set; } = 1;

        [Required]
        [StringLength(20)]
        [Display(Name = "Statut")]
        public string Statut { get; set; } = "EnAttente"; // EnAttente, Disponible, Annulee, Convertie

        [Display(Name = "Date de notification")]
        public DateTime? DateNotification { get; set; }

        // Navigation
        [ForeignKey("IdLivre")]
        public virtual Livre? Livre { get; set; }

        [ForeignKey("IdUtilisateur")]
        public virtual Utilisateur? Utilisateur { get; set; }

        // Propriétés calculées
        [NotMapped]
        public bool EstExpiree => Statut == "Disponible" && DateTime.Now > DateExpiration;

        [NotMapped]
        public int JoursAvantExpiration
        {
            get
            {
                if (Statut != "Disponible") return 0;
                return Math.Max(0, (DateExpiration.Date - DateTime.Today).Days);
            }
        }

        [NotMapped]
        public string StatutAffichage
        {
            get
            {
                return Statut switch
                {
                    "EnAttente" => "En attente",
                    "Disponible" when EstExpiree => "Expirée",
                    "Disponible" => "Disponible",
                    "Annulee" => "Annulée",
                    "Convertie" => "Convertie en emprunt",
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
                    "EnAttente" => "warning",
                    "Disponible" when EstExpiree => "danger",
                    "Disponible" => "success",
                    "Annulee" => "secondary",
                    "Convertie" => "info",
                    _ => "primary"
                };
            }
        }

        [NotMapped]
        public bool PeutEtreAnnulee => Statut == "EnAttente" || Statut == "Disponible";

        [NotMapped]
        public bool PeutEtreConvertie => Statut == "Disponible" && !EstExpiree;
    }
}
