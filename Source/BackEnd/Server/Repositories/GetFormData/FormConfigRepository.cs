using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using Newtonsoft.Json;
using Sinco.Server.Models;

namespace Sinco.Server.Repositories.GetFormData
{
    public class FormConfigRepository : IFormConfigRepository
    {
        private readonly ILogger<FormConfigRepository> _logger;
        private readonly string _connectionString;
        private readonly string _formDirPath;

        public FormConfigRepository(ILogger<FormConfigRepository> logger, IWebHostEnvironment env, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _formDirPath = Path.Combine(env.ContentRootPath, "Controllers", "Form");
        }

        public async Task<string> GetFormConfigAsync(string fileName)
        {
            try
            {
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("File phải là định dạng JSON");
                }

                var filePath = Path.Combine(_formDirPath, fileName);

                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy file cấu hình form: {fileName}");
                }

                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đọc cấu hình form từ file {FileName}", fileName);
                throw;
            }
        }

        public async Task<object> SyncDataFromFormAdvancedAsync(SyncRequest request, dynamic formConfig)
        {
            string procedureName = $"SyncFrom{request.FormId}";
            string idsString = string.Join(",", request.Ids);

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(procedureName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Ids", idsString);
            command.Parameters.AddWithValue("@Unit", request.Unit);
            command.Parameters.AddWithValue("@UserId", request.UserId);
            command.Parameters.AddWithValue("@Language", request.Language);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            // ✅ Bảng đầu tiên: type & message
            if (!await reader.ReadAsync())
                return new { success = false, message = "Không nhận được dữ liệu phản hồi." };

            int type = reader.GetInt32(reader.GetOrdinal("type"));
            string message = reader.IsDBNull(reader.GetOrdinal("message")) ? "" : reader.GetString(reader.GetOrdinal("message"));

            if (type == 0)
            {
                return new
                {
                    success = false,
                    type,
                    message,
                    formConfig = ""
                };
            }

            var resultTables = new List<List<Dictionary<string, object>>>();

            while (await reader.NextResultAsync())
            {
                var table = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                    }
                    table.Add(row);
                }
                resultTables.Add(table);
            }


            // ✅ Gán dữ liệu initialData vào formConfig
            string updatedFormConfig = FormConfigHelper.UpdateInitialDataInFormConfig(
                JsonConvert.SerializeObject(formConfig), resultTables);

            return new
            {
                success = true,
                type,
                message,
                formConfig = JsonConvert.DeserializeObject<dynamic>(updatedFormConfig)
            };
        }



        public async Task<Dictionary<string, object>> GetFormDataAsync(
            string tableName,
            string[] primaryKeys,
            string[] values,
            string unit,
            string userId,
            string type = "list",
            string vcDate = "")
        {
            // This method returns a Dictionary<string, object> which is safe for serialization
            var result = new Dictionary<string, object>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Xử lý tên bảng dựa trên type
                string actualTableName = tableName;
                if (type.ToLower() == "voucher" && !string.IsNullOrEmpty(vcDate))
                {
                    // Chuyển đổi VCDate thành định dạng yyyyMM
                    if (DateTime.TryParse(vcDate, out DateTime parsedDate))
                    {
                        string period = parsedDate.ToString("yyyyMM");
                        actualTableName = $"{tableName}${period}";
                    }
                }

                // Validate table name để tránh SQL injection
                if (!IsValidTableName(actualTableName))
                {
                    throw new ArgumentException($"Tên bảng không hợp lệ: {actualTableName}");
                }

                // Tạo điều kiện WHERE từ primaryKeys và values
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();

                for (int i = 0; i < primaryKeys.Length; i++)
                {
                    // Validate column name để tránh SQL injection
                    if (!IsValidColumnName(primaryKeys[i]))
                    {
                        throw new ArgumentException($"Tên cột không hợp lệ: {primaryKeys[i]}");
                    }

                    whereConditions.Add($"[{primaryKeys[i]}] = @param{i}");
                    parameters.Add(new SqlParameter($"@param{i}", values[i]));
                }

                var whereClause = string.Join(" AND ", whereConditions);
                var query = $"SELECT * FROM [{actualTableName}] WHERE {whereClause}";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Đọc tất cả các cột và giá trị
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var fieldValue = reader.IsDBNull(i) ? null : reader.GetValue(i);

                        result[fieldName] = fieldValue;
                    }
                }

                _logger.LogInformation("Đã lấy dữ liệu từ bảng {TableName} với {RecordCount} bản ghi",
                    actualTableName, result.Count > 0 ? 1 : 0);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi SQL khi lấy dữ liệu từ bảng {TableName}", tableName);
                throw new InvalidOperationException($"Lỗi truy vấn cơ sở dữ liệu: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không mong muốn khi lấy dữ liệu từ bảng {TableName}", tableName);
                throw;
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetDetailDataAsync(
            string tableName,
            string[] foreignKeys,
            string[] values,
            string unit,
            string userId,
            string type = "list",
            string vcDate = "")
        {
            var result = new List<Dictionary<string, object>>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Xử lý tên bảng dựa trên type
                string actualTableName = tableName;
                if (type.ToLower() == "voucher" && !string.IsNullOrEmpty(vcDate))
                {
                    // Chuyển đổi VCDate thành định dạng yyyyMM
                    if (DateTime.TryParse(vcDate, out DateTime parsedDate))
                    {
                        string period = parsedDate.ToString("yyyyMM");
                        actualTableName = $"{tableName}${period}";
                    }
                }

                // Validate table name để tránh SQL injection
                if (!IsValidTableName(actualTableName))
                {
                    throw new ArgumentException($"Tên bảng không hợp lệ: {actualTableName}");
                }

                // Tạo điều kiện WHERE từ foreignKeys và values
                var whereConditions = new List<string>();
                var parameters = new List<SqlParameter>();

                for (int i = 0; i < foreignKeys.Length; i++)
                {
                    // Validate column name để tránh SQL injection
                    if (!IsValidColumnName(foreignKeys[i]))
                    {
                        throw new ArgumentException($"Tên cột không hợp lệ: {foreignKeys[i]}");
                    }

                    // Chỉ thêm điều kiện WHERE nếu value không rỗng
                    if (!string.IsNullOrEmpty(values[i]))
                    {
                        whereConditions.Add($"[{foreignKeys[i]}] = @param{i}");
                        parameters.Add(new SqlParameter($"@param{i}", values[i]));
                    }
                }

                string query;
                if (whereConditions.Count > 0)
                {
                    var whereClause = string.Join(" AND ", whereConditions);
                    query = $"SELECT * FROM [{actualTableName}] WHERE {whereClause} ORDER BY line_nbr";
                }
                else
                {
                    // Nếu không có điều kiện WHERE, lấy tất cả dữ liệu
                    query = $"SELECT * FROM [{actualTableName}] ORDER BY line_nbr";
                }

                using var command = new SqlCommand(query, connection);
                if (parameters.Count > 0)
                {
                    command.Parameters.AddRange(parameters.ToArray());
                }

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();

                    // Đọc tất cả các cột và giá trị
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldName = reader.GetName(i);
                        var fieldValue = reader.IsDBNull(i) ? null : reader.GetValue(i);

                        row[fieldName] = fieldValue;
                    }

                    result.Add(row);
                }

                _logger.LogInformation("Đã lấy dữ liệu detail từ bảng {TableName} với {RecordCount} bản ghi",
                    actualTableName, result.Count);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi SQL khi lấy dữ liệu detail từ bảng {TableName}", tableName);
                throw new InvalidOperationException($"Lỗi truy vấn cơ sở dữ liệu: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không mong muốn khi lấy dữ liệu detail từ bảng {TableName}", tableName);
                throw;
            }

            return result;
        }

        private static bool IsValidTableName(string tableName)
        {
            // Kiểm tra tên bảng chỉ chứa ký tự chữ, số, dấu gạch dưới và dấu $
            return !string.IsNullOrWhiteSpace(tableName) &&
                   System.Text.RegularExpressions.Regex.IsMatch(tableName, @"^[a-zA-Z_][a-zA-Z0-9_$]*$");
        }

        private static bool IsValidColumnName(string columnName)
        {
            // Kiểm tra tên cột chỉ chứa ký tự chữ, số và dấu gạch dưới
            return !string.IsNullOrWhiteSpace(columnName) &&
                   System.Text.RegularExpressions.Regex.IsMatch(columnName, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }
    }
}