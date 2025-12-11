using Bibliotheque.Core.DTOs;
using Bibliotheque.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Frontoffice.MVC.Services
{
    public class EmpruntService : IEmpruntService
    {
        private readonly AdoNetContext _context;

        public EmpruntService(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<List<EmpruntDTO>> GetEmpruntsUtilisateurAsync(int userId)
        {
            var emprunts = new List<EmpruntDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT e.*, l.Titre, l.ImageCouverture, a.Nom + ' ' + ISNULL(a.Prenom, '') AS NomAuteur
                FROM Emprunts e
                INNER JOIN Livres l ON e.IdLivre = l.IdLivre
                LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
                WHERE e.IdUtilisateur = @UserId AND e.Statut IN ('EnCours', 'EnRetard')
                ORDER BY e.DateRetourPrevue";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                emprunts.Add(MapEmpruntDTO(reader));
            }

            return emprunts;
        }

        public async Task<List<EmpruntDTO>> GetHistoriqueUtilisateurAsync(int userId)
        {
            var emprunts = new List<EmpruntDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT e.*, l.Titre, l.ImageCouverture, a.Nom + ' ' + ISNULL(a.Prenom, '') AS NomAuteur
                FROM Emprunts e
                INNER JOIN Livres l ON e.IdLivre = l.IdLivre
                LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
                WHERE e.IdUtilisateur = @UserId
                ORDER BY e.DateEmprunt DESC";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                emprunts.Add(MapEmpruntDTO(reader));
            }

            return emprunts;
        }

        public async Task<bool> ProlongerEmpruntAsync(int empruntId, int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var cmd = new SqlCommand("sp_ProlongerEmprunt", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IdEmprunt", empruntId);

            // Paramètre de sortie pour le résultat
            var resultParam = new SqlParameter("@Resultat", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(resultParam);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return (int)resultParam.Value == 1;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetNombreEmpruntsEnCoursAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT COUNT(*) FROM Emprunts WHERE IdUtilisateur = @UserId AND Statut IN ('EnCours', 'EnRetard')";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            return (int)await cmd.ExecuteScalarAsync();
        }

        private static EmpruntDTO MapEmpruntDTO(SqlDataReader reader)
        {
            return new EmpruntDTO
            {
                IdEmprunt = reader.GetInt32(reader.GetOrdinal("IdEmprunt")),
                IdLivre = reader.GetInt32(reader.GetOrdinal("IdLivre")),
                IdUtilisateur = reader.GetInt32(reader.GetOrdinal("IdUtilisateur")),
                TitreLivre = reader.GetString(reader.GetOrdinal("Titre")),
                ImageCouverture = reader.IsDBNull(reader.GetOrdinal("ImageCouverture")) ? null : reader.GetString(reader.GetOrdinal("ImageCouverture")),
                NomAuteur = reader.IsDBNull(reader.GetOrdinal("NomAuteur")) ? null : reader.GetString(reader.GetOrdinal("NomAuteur")),
                DateEmprunt = reader.GetDateTime(reader.GetOrdinal("DateEmprunt")),
                DateRetourPrevue = reader.GetDateTime(reader.GetOrdinal("DateRetourPrevue")),
                DateRetourEffective = reader.IsDBNull(reader.GetOrdinal("DateRetourEffective")) ? null : reader.GetDateTime(reader.GetOrdinal("DateRetourEffective")),
                Statut = reader.GetString(reader.GetOrdinal("Statut")),
                NombreProlongations = reader.GetInt32(reader.GetOrdinal("NombreProlongations"))
            };
        }
    }
}
