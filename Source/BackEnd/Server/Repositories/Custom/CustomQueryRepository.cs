using Sinco.Server.Models;
using System.Data.SqlClient;

namespace Sinco.Server.Repositories.Custom
{
    public class CustomQueryRepository : ICustomQueryRepository
    {
        private readonly string _connectionString;
        public CustomQueryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Dictionary<string, object>>> ExecuteCustomQueryAsync(CustomQueryRequest request)
        {
            var declareSql = new System.Text.StringBuilder();
            var setSql = new System.Text.StringBuilder();

            for (int i = 0; i < request.Params.Count; i++)
            {
                string param = request.Params[i];
                string type = request.DataType[i];
                string value = request.Value[i];

                string sqlType = type switch
                {
                    "String" => $"nvarchar({Math.Max(64, value.Length)})",
                    "DateTime" => "smalldatetime",
                    "Int" => "numeric(24,4)",
                    _ => "nvarchar(64)"
                };

                declareSql.AppendLine($"declare @{param} {sqlType};");

                string sqlValue = type switch
                {
                    "String" => $"N'{value}'",
                    "DateTime" => $"'{DateTime.Parse(value):yyyyMMdd}'",
                    "Int" => value,
                    _ => $"N'{value}'"
                };

                setSql.AppendLine($"set @{param} = {sqlValue};");
            }

            string finalSql = declareSql.ToString() + setSql.ToString() + request.Query;

            var result = new List<Dictionary<string, object>>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand(finalSql, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.GetValue(i);
                        result.Add(row);
                    }
                }
            }
            return result;
        }
    }
}
