using Bibliotheque.Core.DTOs;
using Bibliotheque.Core.Entities;
using Bibliotheque.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Frontoffice.MVC.Services
{
    public class LivreService : ILivreService
    {
        private readonly AdoNetContext _context;

        public LivreService(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDTO<LivreDTO>> RechercherAsync(string? search, int? categorieId, int page, int pageSize, string tri = "titre")
        {
            var result = new PagedResultDTO<LivreDTO>();
            var items = new List<LivreDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            // Utiliser la procédure stockée
            using var cmd = new SqlCommand("sp_RechercherLivres", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Recherche", (object?)search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategorieId", (object?)categorieId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Tri", tri);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new LivreDTO
                {
                    IdLivre = reader.GetInt32("IdLivre"),
                    ISBN = reader.IsDBNull("ISBN") ? null : reader.GetString("ISBN"),
                    Titre = reader.GetString("Titre"),
                    NomAuteur = reader.IsDBNull("NomAuteur") ? null : reader.GetString("NomAuteur"),
                    Annee = reader.IsDBNull("Annee") ? null : reader.GetInt32("Annee"),
                    Editeur = reader.IsDBNull("Editeur") ? null : reader.GetString("Editeur"),
                    Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                    ImageCouverture = reader.IsDBNull("ImageCouverture") ? null : reader.GetString("ImageCouverture"),
                    Stock = reader.GetInt32("Stock"),
                    StockDisponible = reader.GetInt32("StockDisponible"),
                    NoteMoyenne = reader.IsDBNull("NoteMoyenne") ? 0 : Convert.ToDecimal(reader.GetDouble("NoteMoyenne"))
                });
            }

            // Compter le total
            await reader.NextResultAsync();
            if (await reader.ReadAsync())
            {
                result.TotalCount = reader.GetInt32(0);
            }

            result.Items = items;
            result.Page = page;
            result.PageSize = pageSize;
            // TotalPages est calculé automatiquement

            return result;
        }

        public async Task<LivreDTO?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT l.*, ISNULL(a.Prenom + ' ', '') + a.Nom AS NomAuteur,
                       (SELECT AVG(CAST(Note AS FLOAT)) FROM Avis WHERE IdLivre = l.IdLivre) AS NoteMoyenne,
                       (SELECT COUNT(*) FROM Avis WHERE IdLivre = l.IdLivre) AS NombreAvis
                FROM Livres l
                LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
                WHERE l.IdLivre = @Id AND l.Actif = 1";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new LivreDTO
                {
                    IdLivre = reader.GetInt32(reader.GetOrdinal("IdLivre")),
                    ISBN = reader.IsDBNull(reader.GetOrdinal("ISBN")) ? null : reader.GetString(reader.GetOrdinal("ISBN")),
                    Titre = reader.GetString(reader.GetOrdinal("Titre")),
                    NomAuteur = reader.IsDBNull(reader.GetOrdinal("NomAuteur")) ? null : reader.GetString(reader.GetOrdinal("NomAuteur")),
                    IdAuteur = reader.IsDBNull(reader.GetOrdinal("IdAuteur")) ? null : reader.GetInt32(reader.GetOrdinal("IdAuteur")),
                    Annee = reader.IsDBNull(reader.GetOrdinal("Annee")) ? null : reader.GetInt32(reader.GetOrdinal("Annee")),
                    Editeur = reader.IsDBNull(reader.GetOrdinal("Editeur")) ? null : reader.GetString(reader.GetOrdinal("Editeur")),
                    NombrePages = reader.IsDBNull(reader.GetOrdinal("NombrePages")) ? null : reader.GetInt32(reader.GetOrdinal("NombrePages")),
                    Langue = reader.IsDBNull(reader.GetOrdinal("Langue")) ? null : reader.GetString(reader.GetOrdinal("Langue")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    ImageCouverture = reader.IsDBNull(reader.GetOrdinal("ImageCouverture")) ? null : reader.GetString(reader.GetOrdinal("ImageCouverture")),
                    Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                    StockDisponible = reader.GetInt32(reader.GetOrdinal("StockDisponible")),
                    NoteMoyenne = reader.IsDBNull(reader.GetOrdinal("NoteMoyenne")) ? 0 : Convert.ToDecimal(reader["NoteMoyenne"]),
                    NombreAvis = reader.IsDBNull(reader.GetOrdinal("NombreAvis")) ? 0 : reader.GetInt32(reader.GetOrdinal("NombreAvis"))
                };
            }

            return null;
        }

        public async Task<List<Categorie>> GetCategoriesAsync()
        {
            var categories = new List<Categorie>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM Categories WHERE Actif = 1 ORDER BY Nom";
            using var cmd = new SqlCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new Categorie
                {
                    IdCategorie = reader.GetInt32(reader.GetOrdinal("IdCategorie")),
                    Nom = reader.GetString(reader.GetOrdinal("Nom")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Couleur = reader.IsDBNull(reader.GetOrdinal("Couleur")) ? "#6c757d" : reader.GetString(reader.GetOrdinal("Couleur"))
                });
            }

            return categories;
        }

        public async Task<List<LivreDTO>> GetRecommandationsAsync(int userId)
        {
            var livres = new List<LivreDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_GetRecommandations", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdUtilisateur", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                livres.Add(MapLivreDTO(reader));
            }

            return livres;
        }

        public async Task<List<LivreDTO>> GetNouveautesAsync(int count = 10)
        {
            var livres = new List<LivreDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT TOP (@Count) l.*, ISNULL(a.Prenom + ' ', '') + a.Nom AS NomAuteur
                FROM Livres l
                LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
                WHERE l.Actif = 1
                ORDER BY l.DateAjout DESC";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                livres.Add(MapLivreDTO(reader));
            }

            return livres;
        }

        public async Task<List<LivreDTO>> GetPopulairesAsync(int count = 10)
        {
            var livres = new List<LivreDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT TOP (@Count) l.IdLivre, l.ISBN, l.Titre, l.IdAuteur, l.Annee, l.Editeur,
                       l.NombrePages, l.Langue, l.Description, l.ImageCouverture, l.Stock,
                       l.StockDisponible, l.Emplacement, l.DateAjout, l.Actif,
                       ISNULL(a.Prenom + ' ', '') + a.Nom AS NomAuteur,
                       COUNT(e.IdEmprunt) AS NombreEmprunts
                FROM Livres l
                LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
                LEFT JOIN Emprunts e ON l.IdLivre = e.IdLivre
                WHERE l.Actif = 1
                GROUP BY l.IdLivre, l.ISBN, l.Titre, l.IdAuteur, l.Annee, l.Editeur, l.NombrePages,
                         l.Langue, l.Description, l.ImageCouverture, l.Stock, l.StockDisponible,
                         l.Emplacement, l.DateAjout, l.Actif, a.Nom, a.Prenom
                ORDER BY COUNT(e.IdEmprunt) DESC";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Count", count);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                livres.Add(MapLivreDTO(reader));
            }

            return livres;
        }

        private static LivreDTO MapLivreDTO(SqlDataReader reader)
        {
            return new LivreDTO
            {
                IdLivre = reader.GetInt32(reader.GetOrdinal("IdLivre")),
                ISBN = reader.IsDBNull(reader.GetOrdinal("ISBN")) ? null : reader.GetString(reader.GetOrdinal("ISBN")),
                Titre = reader.GetString(reader.GetOrdinal("Titre")),
                NomAuteur = reader.IsDBNull(reader.GetOrdinal("NomAuteur")) ? null : reader.GetString(reader.GetOrdinal("NomAuteur")),
                Annee = reader.IsDBNull(reader.GetOrdinal("Annee")) ? null : reader.GetInt32(reader.GetOrdinal("Annee")),
                ImageCouverture = reader.IsDBNull(reader.GetOrdinal("ImageCouverture")) ? null : reader.GetString(reader.GetOrdinal("ImageCouverture")),
                Stock = reader.GetInt32(reader.GetOrdinal("Stock")),
                StockDisponible = reader.GetInt32(reader.GetOrdinal("StockDisponible"))
            };
        }
    }
}
