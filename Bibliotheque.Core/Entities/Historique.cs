using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Historiques")]
    public class Historique
    {
        [Key]
        public int IdHistorique { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Table")]
        public string TableNom { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ID de l'enregistrement")]
        public int IdEnregistrement { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Action")]
        public string Action { get; set; } = string.Empty; // Insert, Update, Delete

        [Display(Name = "Ancienne valeur")]
        public string? AncienneValeur { get; set; }

        [Display(Name = "Nouvelle valeur")]
        public string? NouvelleValeur { get; set; }

        [Display(Name = "Admin")]
        public int? IdAdmin { get; set; }

        [Display(Name = "Date de l'action")]
        public DateTime DateAction { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "Adresse IP")]
        public string? AdresseIP { get; set; }

        // Navigation
        [ForeignKey("IdAdmin")]
        public virtual Admin? Admin { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string ActionIcone
        {
            get
            {
                return Action switch
                {
                    "Insert" => "bi-plus-circle text-success",
                    "Update" => "bi-pencil text-warning",
                    "Delete" => "bi-trash text-danger",
                    _ => "bi-question-circle text-info"
                };
            }
        }

        [NotMapped]
        public string ActionTexte
        {
            get
            {
                return Action switch
                {
                    "Insert" => "Création",
                    "Update" => "Modification",
                    "Delete" => "Suppression",
                    _ => Action
                };
            }
        }
    }
}
