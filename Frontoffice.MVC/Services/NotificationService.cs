using Bibliotheque.Core.Entities;
using Bibliotheque.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Frontoffice.MVC.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AdoNetContext _context;

        public NotificationService(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetNotificationsUtilisateurAsync(int userId)
        {
            var notifications = new List<Notification>();

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                SELECT * FROM Notifications
                WHERE IdUtilisateur = @UserId
                ORDER BY DateCreation DESC";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                notifications.Add(new Notification
                {
                    IdNotification = reader.GetInt32(reader.GetOrdinal("IdNotification")),
                    IdUtilisateur = reader.GetInt32(reader.GetOrdinal("IdUtilisateur")),
                    Type = reader.GetString(reader.GetOrdinal("Type")),
                    Message = reader.GetString(reader.GetOrdinal("Message")),
                    DateCreation = reader.GetDateTime(reader.GetOrdinal("DateCreation")),
                    Lu = reader.GetBoolean(reader.GetOrdinal("Lu"))
                });
            }

            return notifications;
        }

        public async Task<int> GetNombreNonLuesAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT COUNT(*) FROM Notifications WHERE IdUtilisateur = @UserId AND Lu = 0";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task MarquerCommeLueAsync(int notificationId, int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "UPDATE Notifications SET Lu = 1 WHERE IdNotification = @Id AND IdUtilisateur = @UserId";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", notificationId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarquerToutesCommeLuesAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "UPDATE Notifications SET Lu = 1 WHERE IdUtilisateur = @UserId AND Lu = 0";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
