using Bibliotheque.Core.Entities;
using Bibliotheque.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace Frontoffice.MVC.Services
{
    public class UtilisateurService : IUtilisateurService
    {
        private readonly AdoNetContext _context;

        public UtilisateurService(AdoNetContext context)
        {
            _context = context;
        }

        public async Task<Utilisateur?> AuthenticateAsync(string email, string password)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM Utilisateurs WHERE Email = @Email AND Actif = 1 AND EstBloque = 0";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Email", email);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var hash = reader.GetString(reader.GetOrdinal("MotDePasseHash"));

                if (BCrypt.Net.BCrypt.Verify(password, hash))
                {
                    return new Utilisateur
                    {
                        IdUtilisateur = reader.GetInt32(reader.GetOrdinal("IdUtilisateur")),
                        Nom = reader.GetString(reader.GetOrdinal("Nom")),
                        Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString(reader.GetOrdinal("Telephone")),
                        Adresse = reader.IsDBNull(reader.GetOrdinal("Adresse")) ? null : reader.GetString(reader.GetOrdinal("Adresse")),
                        DateInscription = reader.GetDateTime(reader.GetOrdinal("DateInscription")),
                        Actif = reader.GetBoolean(reader.GetOrdinal("Actif")),
                        EstBloque = reader.GetBoolean(reader.GetOrdinal("EstBloque"))
                    };
                }
            }

            return null;
        }

        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT * FROM Utilisateurs WHERE IdUtilisateur = @Id";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Utilisateur
                {
                    IdUtilisateur = reader.GetInt32(reader.GetOrdinal("IdUtilisateur")),
                    Nom = reader.GetString(reader.GetOrdinal("Nom")),
                    Prenom = reader.GetString(reader.GetOrdinal("Prenom")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Telephone = reader.IsDBNull(reader.GetOrdinal("Telephone")) ? null : reader.GetString(reader.GetOrdinal("Telephone")),
                    Adresse = reader.IsDBNull(reader.GetOrdinal("Adresse")) ? null : reader.GetString(reader.GetOrdinal("Adresse")),
                    DateInscription = reader.GetDateTime(reader.GetOrdinal("DateInscription")),
                    Actif = reader.GetBoolean(reader.GetOrdinal("Actif")),
                    EstBloque = reader.GetBoolean(reader.GetOrdinal("EstBloque"))
                };
            }

            return null;
        }

        public async Task<bool> RegisterAsync(string nom, string prenom, string email, string password, string? telephone)
        {
            if (await EmailExistsAsync(email))
                return false;

            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO Utilisateurs (Nom, Prenom, Email, MotDePasseHash, Telephone, DateInscription, Actif, EstBloque)
                VALUES (@Nom, @Prenom, @Email, @MotDePasseHash, @Telephone, @DateInscription, 1, 0)";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@Prenom", prenom);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@MotDePasseHash", BCrypt.Net.BCrypt.HashPassword(password));
            cmd.Parameters.AddWithValue("@Telephone", (object?)telephone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DateInscription", DateTime.Now);

            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> UpdateProfileAsync(int userId, string nom, string prenom, string? telephone, string? adresse)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = @"
                UPDATE Utilisateurs
                SET Nom = @Nom, Prenom = @Prenom, Telephone = @Telephone, Adresse = @Adresse
                WHERE IdUtilisateur = @Id";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", userId);
            cmd.Parameters.AddWithValue("@Nom", nom);
            cmd.Parameters.AddWithValue("@Prenom", prenom);
            cmd.Parameters.AddWithValue("@Telephone", (object?)telephone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Adresse", (object?)adresse ?? DBNull.Value);

            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            // Vérifier l'ancien mot de passe
            var checkCmd = new SqlCommand("SELECT MotDePasseHash FROM Utilisateurs WHERE IdUtilisateur = @Id", connection);
            checkCmd.Parameters.AddWithValue("@Id", userId);

            var hash = await checkCmd.ExecuteScalarAsync() as string;
            if (hash == null || !BCrypt.Net.BCrypt.Verify(oldPassword, hash))
                return false;

            // Mettre à jour le mot de passe
            var updateCmd = new SqlCommand("UPDATE Utilisateurs SET MotDePasseHash = @Hash WHERE IdUtilisateur = @Id", connection);
            updateCmd.Parameters.AddWithValue("@Id", userId);
            updateCmd.Parameters.AddWithValue("@Hash", BCrypt.Net.BCrypt.HashPassword(newPassword));

            var result = await updateCmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            var sql = "SELECT COUNT(*) FROM Utilisateurs WHERE Email = @Email";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Email", email);

            var count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }
}
