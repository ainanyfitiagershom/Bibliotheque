using Bibliotheque.Core.DTOs;
using Bibliotheque.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Frontoffice.MVC.Services
{
    public class ReservationService : IReservationService
    {
        private readonly AdoNetContext _context;

        public ReservationService(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<List<ReservationDTO>> GetReservationsUtilisateurAsync(int userId)
        {
            var reservations = new List<ReservationDTO>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT r.*, l.Titre, l.ImageCouverture, a.Nom + ' ' + ISNULL(a.Prenom, '') AS NomAuteur,
                       (SELECT COUNT(*) FROM Reservations r2
                        WHERE r2.IdLivre = r.IdLivre AND r2.Statut = 'EnAttente'
                        AND r2.DateReservation < r.DateReservation) + 1 AS PositionFile
                FROM Reservations r
                INNER JOIN Livres l ON r.IdLivre = l.IdLivre
                LEFT JOIN Auteurs a ON l.IdAuteur = a.IdAuteur
                WHERE r.IdUtilisateur = @UserId AND r.Statut IN ('EnAttente', 'Disponible')
                ORDER BY r.DateReservation";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reservations.Add(new ReservationDTO
                {
                    IdReservation = reader.GetInt32(reader.GetOrdinal("IdReservation")),
                    IdLivre = reader.GetInt32(reader.GetOrdinal("IdLivre")),
                    IdUtilisateur = reader.GetInt32(reader.GetOrdinal("IdUtilisateur")),
                    TitreLivre = reader.GetString(reader.GetOrdinal("Titre")),
                    ImageCouverture = reader.IsDBNull(reader.GetOrdinal("ImageCouverture")) ? null : reader.GetString(reader.GetOrdinal("ImageCouverture")),
                    NomAuteur = reader.IsDBNull(reader.GetOrdinal("NomAuteur")) ? null : reader.GetString(reader.GetOrdinal("NomAuteur")),
                    DateReservation = reader.GetDateTime(reader.GetOrdinal("DateReservation")),
                    DateExpiration = reader.IsDBNull(reader.GetOrdinal("DateExpiration")) ? null : reader.GetDateTime(reader.GetOrdinal("DateExpiration")),
                    Statut = reader.GetString(reader.GetOrdinal("Statut")),
                    PositionFile = reader.GetInt32(reader.GetOrdinal("PositionFile"))
                });
            }

            return reservations;
        }

        public async Task<bool> ReserverAsync(int livreId, int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            // Vérifier si l'utilisateur n'a pas déjà réservé ce livre
            var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Reservations WHERE IdLivre = @LivreId AND IdUtilisateur = @UserId AND Statut IN ('EnAttente', 'Disponible')",
                connection);
            checkCmd.Parameters.AddWithValue("@LivreId", livreId);
            checkCmd.Parameters.AddWithValue("@UserId", userId);

            if ((int)await checkCmd.ExecuteScalarAsync() > 0)
                return false;

            // Vérifier si l'utilisateur n'a pas déjà emprunté ce livre
            var checkEmpruntCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Emprunts WHERE IdLivre = @LivreId AND IdUtilisateur = @UserId AND Statut IN ('EnCours', 'EnRetard')",
                connection);
            checkEmpruntCmd.Parameters.AddWithValue("@LivreId", livreId);
            checkEmpruntCmd.Parameters.AddWithValue("@UserId", userId);

            if ((int)await checkEmpruntCmd.ExecuteScalarAsync() > 0)
                return false;

            // Créer la réservation
            var sql = @"
                INSERT INTO Reservations (IdLivre, IdUtilisateur, DateReservation, Statut)
                VALUES (@LivreId, @UserId, @DateReservation, 'EnAttente')";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@LivreId", livreId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@DateReservation", DateTime.Now);

            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> AnnulerReservationAsync(int reservationId, int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                UPDATE Reservations
                SET Statut = 'Annulee'
                WHERE IdReservation = @Id AND IdUtilisateur = @UserId AND Statut IN ('EnAttente', 'Disponible')";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", reservationId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<int> GetPositionFileAttenteAsync(int livreId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT COUNT(*) FROM Reservations WHERE IdLivre = @LivreId AND Statut = 'EnAttente'";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@LivreId", livreId);

            return (int)await cmd.ExecuteScalarAsync();
        }
    }
}
