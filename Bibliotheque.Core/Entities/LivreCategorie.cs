using System.ComponentModel.DataAnnotations.Schema;

namespace Bibliotheque.Core.Entities
{
    /// <summary>
    /// Table de liaison pour la relation N:N entre Livres et Categories
    /// </summary>
    [Table("LivreCategories")]
    public class LivreCategorie
    {
        public int IdLivre { get; set; }
        public int IdCategorie { get; set; }

        // Navigation
        [ForeignKey("IdLivre")]
        public virtual Livre? Livre { get; set; }

        [ForeignKey("IdCategorie")]
        public virtual Categorie? Categorie { get; set; }
    }
}
