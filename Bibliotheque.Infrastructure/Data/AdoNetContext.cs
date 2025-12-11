using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Bibliotheque.Infrastructure.Data
{
    /// <summary>
    /// Contexte ADO.NET pour les requêtes directes (Frontoffice)
    /// </summary>
    public class AdoNetContext
    {
        private readonly string _connectionString;

        public AdoNetContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public AdoNetContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Créer et retourner une nouvelle connexion SQL
        /// </summary>
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Exécuter une requête et retourner un DataReader
        /// </summary>
        public async Task<SqlDataReader> ExecuteReaderAsync(string query, params SqlParameter[] parameters)
        {
            var connection = CreateConnection();
            await connection.OpenAsync();

            var command = new SqlCommand(query, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Exécuter une requête et retourner le nombre de lignes affectées
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Exécuter une requête et retourner une valeur scalaire
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string query, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteScalarAsync();
        }

        /// <summary>
        /// Exécuter une procédure stockée
        /// </summary>
        public async Task<SqlDataReader> ExecuteStoredProcedureAsync(string procedureName, params SqlParameter[] parameters)
        {
            var connection = CreateConnection();
            await connection.OpenAsync();

            var command = new SqlCommand(procedureName, connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Exécuter une procédure stockée sans retour
        /// </summary>
        public async Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, params SqlParameter[] parameters)
        {
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            return await command.ExecuteNonQueryAsync();
        }
    }
}
