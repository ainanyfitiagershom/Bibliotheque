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

        public async Task<(bool Success, string? ErrorMessage)> ProlongerEmpruntAsync(int empruntId, int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            // Vérifier d'abord que l'emprunt appartient à l'utilisateur et récupérer les infos
            var checkCmd = new SqlCommand(@"
                SELECT e.IdEmprunt, e.IdLivre, e.NombreProlongations, e.MaxProlongations, e.Statut
                FROM Emprunts e
                WHERE e.IdEmprunt = @IdEmprunt AND e.IdUtilisateur = @UserId AND e.Statut IN ('EnCours', 'EnRetard')",
                connection);
            checkCmd.Parameters.AddWithValue("@IdEmprunt", empruntId);
            checkCmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await checkCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return (false, "Emprunt non trouvé.");
            }

            var idLivre = reader.GetInt32(reader.GetOrdinal("IdLivre"));
            var nombreProlongations = reader.GetInt32(reader.GetOrdinal("NombreProlongations"));
            var maxProlongations = reader.GetInt32(reader.GetOrdinal("MaxProlongations"));
            var statut = reader.GetString(reader.GetOrdinal("Statut"));
            await reader.CloseAsync();

            // Vérifier le nombre de prolongations
            if (nombreProlongations >= maxProlongations)
            {
                return (false, $"Vous avez atteint le nombre maximum de prolongations ({maxProlongations}).");
            }

            // Vérifier s'il y a des réservations en attente pour ce livre
            var reservationCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Reservations WHERE IdLivre = @IdLivre AND Statut = 'EnAttente'",
                connection);
            reservationCmd.Parameters.AddWithValue("@IdLivre", idLivre);
            var reservationsEnAttente = (int)await reservationCmd.ExecuteScalarAsync();

            if (reservationsEnAttente > 0)
            {
                return (false, $"Impossible de prolonger : {reservationsEnAttente} personne(s) attendent ce livre en réservation.");
            }

            // Effectuer la prolongation directement sans passer par la procédure stockée
            try
            {
                var updateCmd = new SqlCommand(@"
                    UPDATE Emprunts SET
                        DateRetourPrevue = DATEADD(DAY, 7, DateRetourPrevue),
                        NombreProlongations = NombreProlongations + 1,
                        Statut = 'EnCours'
                    WHERE IdEmprunt = @IdEmprunt",
                    connection);
                updateCmd.Parameters.AddWithValue("@IdEmprunt", empruntId);

                var rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? (true, null) : (false, "Erreur lors de la mise à jour.");
            }
            catch (SqlException ex)
            {
                return (false, $"Erreur SQL : {ex.Message}");
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
