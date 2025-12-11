using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    [Table("Avis")]
    public class Avis
    {
        [Key]
        public int IdAvis { get; set; }

        [Required(ErrorMessage = "Le livre est obligatoire")]
        [Display(Name = "Livre")]
        public int IdLivre { get; set; }

        [Required(ErrorMessage = "L'utilisateur est obligatoire")]
        [Display(Name = "Utilisateur")]
        public int IdUtilisateur { get; set; }

        [Required(ErrorMessage = "La note est obligatoire")]
        [Range(1, 5, ErrorMessage = "La note doit être entre 1 et 5")]
        [Display(Name = "Note")]
        public int Note { get; set; }

        [Display(Name = "Commentaire")]
        public string? Commentaire { get; set; }

        [Display(Name = "Date de l'avis")]
        public DateTime DateAvis { get; set; } = DateTime.Now;

        [Display(Name = "Approuvé")]
        public bool Approuve { get; set; } = false;

        // Navigation
        [ForeignKey("IdLivre")]
        public virtual Livre? Livre { get; set; }

        [ForeignKey("IdUtilisateur")]
        public virtual Utilisateur? Utilisateur { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string EtoilesHtml
        {
            get
            {
                var pleines = string.Concat(Enumerable.Repeat("★", Note));
                var vides = string.Concat(Enumerable.Repeat("☆", 5 - Note));
                return pleines + vides;
            }
        }

        [NotMapped]
        public string NoteTexte
        {
            get
            {
                return Note switch
                {
                    1 => "Très mauvais",
                    2 => "Mauvais",
                    3 => "Moyen",
                    4 => "Bon",
                    5 => "Excellent",
                    _ => "Non noté"
                };
            }
        }
    }
}
