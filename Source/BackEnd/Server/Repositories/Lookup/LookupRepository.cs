using Dapper;
using Sinco.Server.Controllers;
using System.Data.SqlClient;
using System.Text.Json;

namespace Sinco.Server.Repositories.Lookup
{
    public class LookupRepository : ILookupRepository
    {
        private readonly string _lookupFolder;
        private readonly string _connectionString;

        public LookupRepository(IConfiguration config)
        {
            _lookupFolder = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Form", "Lookup");
            _connectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<object> LookupAsync(string controller, List<LookupController.FilterItem> filters, string language = "", string unit = "", string userId = "")
        {
            try
            {
                var filePath = Path.Combine(_lookupFolder, $"Lookup{char.ToUpper(controller[0]) + controller.Substring(1)}.json");
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Không tìm thấy file cấu hình lookup: {filePath}");

                var jsonMeta = await File.ReadAllTextAsync(filePath);
                
                // Replace placeholders with actual values
                jsonMeta = jsonMeta.Replace("@language", language)
                                 .Replace("@@unit", unit)
                                 .Replace("@@userId", userId);
                
                var metaDoc = JsonDocument.Parse(jsonMeta).RootElement;

                // Đọc metadata
                var table = metaDoc.GetProperty("formId").GetString() ?? throw new InvalidOperationException("formId not found");
                var type = metaDoc.GetProperty("type").GetString() ?? "many";

                // Handle sort as array or string
                var sortFields = new List<string>();
                if (metaDoc.TryGetProperty("sort", out var sortProp))
                {
                    if (sortProp.ValueKind == JsonValueKind.Array)
                    {
                        sortFields = sortProp.EnumerateArray()
                            .Select(s => s.GetString())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }
                    else
                    {
                        var sortField = sortProp.GetString();
                        if (!string.IsNullOrEmpty(sortField))
                            sortFields.Add(sortField);
                    }
                }
                
                // Default sort if none specified
                if (!sortFields.Any())
                    sortFields.Add("1");

                var fields = metaDoc.GetProperty("fields").EnumerateArray()
                    .Select(f => f.GetProperty("field").GetString())
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToList();

                if (!fields.Any())
                    throw new InvalidOperationException("Không tìm thấy fields trong metadata");

                var selectFields = string.Join(", ", fields);

                // Build SQL động
                var sql = $"SELECT {selectFields} FROM {table}";
                var param = new DynamicParameters();

                // Xử lý filters
                if (filters.Any())
                {
                    var whereConditions = new List<string>();
                    foreach (var filter in filters)
                    {
                        if (!string.IsNullOrEmpty(filter.Field) && !string.IsNullOrEmpty(filter.Operator))
                        {
                            whereConditions.Add($"{filter.Field} {filter.Operator} @{filter.Field}");
                            param.Add($"@{filter.Field}", filter.Value);
                        }
                    }

                    if (whereConditions.Any())
                        sql += $" WHERE {string.Join(" AND ", whereConditions)}";
                }

                sql += $" ORDER BY {string.Join(", ", sortFields)}";

                // Thực thi SQL
                using var conn = new SqlConnection(_connectionString);
                var datas = (await conn.QueryAsync(sql, param)).ToList();

                // Build response object
                var result = new Dictionary<string, object?>();

                // Copy tất cả properties từ metadata (trừ fields)
                foreach (var prop in metaDoc.EnumerateObject())
                {
                    if (prop.Name != "fields")
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            // Handle arrays (primaryKey, sort)
                            result[prop.Name] = prop.Value.EnumerateArray()
                                .Select(item => item.GetString() ?? item.ToString())
                                .ToList();
                        }
                        else
                        {
                            result[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                                ? prop.Value.GetString()
                                : prop.Value.ToString();
                        }
                    }
                }

                // Xử lý fields array
                result["fields"] = metaDoc.GetProperty("fields").EnumerateArray()
                    .Select(f => f.EnumerateObject().ToDictionary(
                        x => x.Name,
                        x => x.Value.GetString() ?? ""
                    ))
                    .ToList();

                // Xử lý datas dựa trên type    
                result["datas"] = datas; // Trả về array
                return result;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Lỗi parse JSON metadata: {ex.Message}", ex);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Lỗi database: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không xác định: {ex.Message}", ex);
            }
        }
    }
}