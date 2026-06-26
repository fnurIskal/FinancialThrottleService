using FinancialThrottleService.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace FinancialThrottleService.Infrastructure.Persistence.Sql
{
    public class SqlFinancialTransactionApi : IFinancialTransactionApi
    {
        private readonly string _baseConnectionString;
        private readonly IConfiguration _configuration;

        public SqlFinancialTransactionApi(IConfiguration configuration)
        {
            _configuration = configuration;
            _baseConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is missing.");
        }

        public async Task<string> GenerateInflationAsync(
            string database, int quarter, int securityId, int templateId, string commands)
        {
            return await ExecuteCommandsAsync("GenerateInflation", database, quarter, securityId, templateId, commands);
        }

        public async Task<string> GenerateRestatedAsync(
            string database, int quarter, int securityId, int templateId, string commands)
        {
            return await ExecuteCommandsAsync("GenerateRestated", database, quarter, securityId, templateId, commands);
        }

        private async Task<string> ExecuteCommandsAsync(
            string operationName, string database, int quarter, int securityId, int templateId, string commands)
        {
            if (string.IsNullOrWhiteSpace(commands))
            {
                Console.WriteLine($"[WARN] {operationName}: no commands to execute for {database}/{securityId}/Q{quarter}/T{templateId}");
                return string.Empty;
            }

            var connectionString = BuildConnectionString(database);

            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(commands, connection)
                {
                    CommandTimeout = 120
                };

                var result = await command.ExecuteNonQueryAsync();

                Console.WriteLine($"[INFO] {operationName}: executed against {database}/{securityId}/Q{quarter}/T{templateId}, rows affected={result}");
                return $"OK rows_affected={result}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {operationName} failed for {database}/{securityId}/Q{quarter}: {ex.Message}");
                throw;
            }
        }

        private string BuildConnectionString(string database)
        {
            var resolved = _configuration[$"DatabaseNames:{database}"] ?? database;
            var builder = new SqlConnectionStringBuilder(_baseConnectionString)
            {
                InitialCatalog = resolved
            };
            return builder.ConnectionString;
        }
    }
}
