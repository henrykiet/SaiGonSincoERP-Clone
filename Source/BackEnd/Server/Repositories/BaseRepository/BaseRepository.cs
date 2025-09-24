using Dapper;
using DocumentFormat.OpenXml.Office.Word;
using FuzzySharp;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using reportSystem01.Server.Data;
using Sinco.Server.Helpers;
using Sinco.Server.Models;
using Sinco.Server.SqlJsonDefinations;
using Sinco.Server.SqlJsonDefinations.Responses;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Sinco.Server.SqlJsonDefinations.SqlJsonDefination;


namespace Sinco.Server.Repositories.BaseRepository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ReportServerContext _context;
        private readonly IDbConnection _dbConnect;
        private readonly DbSet<T> _dbSet;
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseRepository<T>));

        public BaseRepository(ReportServerContext context)
        {
            _context = context;
            _dbConnect = context.Database.GetDbConnection();
            _dbSet = _context.Set<T>();
        }
        /// <summary>
        /// Hàm lấy thông tin của một bảng
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<List<dynamic>> GetAllAsync(string tableName, int? page, int? pageSize)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException(nameof(tableName));
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 100;
            try
            {
                if (_dbConnect.State != ConnectionState.Open)
                    _dbConnect.Open();
                var sql = "";
                // Lấy tổng số dòng
                string countSql = $"SELECT COUNT(*) FROM [{tableName}]";
                int totalCount = await _dbConnect.ExecuteScalarAsync<int>(countSql);
                if (page != null && pageSize != null && page >= 0 && pageSize >= 0)
                {
                    // Lấy dữ liệu trang hiện tại
                    int offset = ((int)page - 1) * (int)pageSize;
                    sql = $@"
								SELECT * 
								FROM [{tableName}] 
								ORDER BY (SELECT NULL) -- Tránh lỗi nếu không có cột cụ thể
								OFFSET {offset} ROWS 
								FETCH NEXT {pageSize} ROWS ONLY";
                }
                else
                {
                    sql = $"SELECT * FROM [{tableName}]";
                }
                var items = await _dbConnect.QueryAsync(sql);
                return items.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception when fetching paged data from table '{tableName}'", ex);
            }
        }

        public async Task<Dictionary<string, List<Dictionary<string, object>>>> GetTablesAsync(Dictionary<string, List<string>> keyValues)
        {
            var result = new Dictionary<string, List<Dictionary<string, object>>>();

            using var connection = new SqlConnection(_dbConnect.ConnectionString);
            await connection.OpenAsync();

            foreach (var table in keyValues)
            {
                var tableName = table.Key;
                var columns = table.Value;

                //bỏ qua bảng không có cột
                if (columns == null || columns.Count == 0) continue;

                //tạo truy vấn
                var columnList = string.Join(",", columns.Select(c => $"[{c}]"));
                var query = $"SELECT {columnList} FROM [{tableName}]";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var tableData = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    foreach (var col in columns)
                    {
                        row[col] = reader[col];
                    }
                    tableData.Add(row);
                }
                result[tableName] = tableData;

                reader.Close();
            }
            return result;
        }
        /// <summary>
        /// Hàm lấy thông tin dựa trên ID 
        /// </summary>
        /// <param name="id">value primarykey</param>
        /// <param name="tableName">table name</param>
        /// <param name="keyName">key name if exist</param>
        /// <returns>list or T if exist</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<dynamic?> GetByIdAsync(object id, string tableName, string? keyName = null)
        {
            if (id == null)
                throw new ExceptionFormat("Giá trị khoá chính là null");

            var sqlDef = await GetTemplateByTableNameAsync(tableName);
            if (sqlDef == null)
                throw new ExceptionFormat($"Không tìm thấy định nghĩa bảng {tableName}");

            // Nếu id là Dictionary → dùng multi-key
            if (id is IDictionary<string, string> keyDict)
            {
                var whereClause = string.Join(" AND ", keyDict.Select(k => $"[{k.Key}] = @{k.Key}"));
                var sql = $"SELECT * FROM [{tableName}] WHERE {whereClause}";
                var parameters = new DynamicParameters();
                foreach (var kv in keyDict)
                {
                    parameters.Add(kv.Key, kv.Value);
                }
                if (_dbConnect.State != ConnectionState.Open)
                    _dbConnect.Open();
                var results = await _dbConnect.QueryAsync<dynamic>(sql, parameters);
                return results;
            }
            else if (id is string)
            {
                // Nếu là single key
                if (string.IsNullOrEmpty(keyName))
                {
                    keyName = GetPrimaryKeys(sqlDef).FirstOrDefault();
                    if (string.IsNullOrEmpty(keyName))
                        throw new ExceptionFormat("Không tìm thấy khóa chính");
                }

                var sqlSingle = $"SELECT * FROM [{tableName}] WHERE [{keyName}] = @id";

                if (_dbConnect.State != ConnectionState.Open)
                    _dbConnect.Open();
                var result = await _dbConnect.QueryAsync<dynamic>(sqlSingle, new { id });
                return result;
            }
            else
            {
                throw new ExceptionFormat("Dữ liệu id truyền vào không hợp lệ");
            }
        }
        public async Task<int> InsertAsync(dynamic entity, string tableName)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));
            var def = await GetTemplateByTableNameAsync(tableName);
            try
            {
                await ValidateEntityAsync(entity, def);
                // Chuyển dynamic entity về dictionary để dễ thao tác
                if (entity is not IDictionary<string, object> dict)
                    throw new Exception("Entity must be a dictionary-like object.");
                var columnName = string.Join(", ", dict.Keys.Select(k => $"[{k}]"));
                var valueName = string.Join(", ", dict.Keys.Select(k => $"@{k}"));
                var sql = $"INSERT INTO [{tableName}] ({columnName}) VALUES ({valueName})";
                var parameters = new DynamicParameters();
                foreach (var param in dict)
                {
                    parameters.Add($"@{param.Key}", param.Value);
                }
                return await _dbConnect.ExecuteAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception when insert: ", ex);
            }
        }
        public async Task<int> UpdateAsync(T entity, string tableName)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
            var def = await GetTemplateByTableNameAsync(tableName);
            try
            {
                await ValidateEntityAsync(entity, def);
                if (entity is not IDictionary<string, object> dict)
                    throw new Exception("Entity must be a dictionary-like object.");
                var keyFields = def.Schema.Fields.Where(f => f.PrimaryKey == true).ToList();
                if (keyFields.Count == 0)
                    throw new Exception("No primary key defined for the table.");
                var setClauses = dict
                    .Where(kv => !keyFields.Any(kf => kf.Property.Equals(kv.Key, StringComparison.OrdinalIgnoreCase)))
                    .Select(kv => $"[{kv.Key}] = @{kv.Key}");

                var whereClauses = keyFields
                    .Select(f => $"[{f.Property}] = @{f.Property}");
                var sql = $@"
							UPDATE [{tableName}]
							SET {string.Join(", ", setClauses)}
							WHERE {string.Join(" AND ", whereClauses)}";
                var parameters = new DynamicParameters();
                foreach (var param in dict)
                {
                    parameters.Add($"@{param.Key}", param.Value);
                }
                return await _dbConnect.ExecuteAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception when updating:", ex);
            }
        }
        public async Task<int> DeleteAsync(string id, string tableName)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
            var def = await GetTemplateByTableNameAsync(tableName);
            try
            {
                var keyFields = def.Schema.Fields.Where(f => f.PrimaryKey == true).ToList();
                if (keyFields.Count == 0)
                    throw new Exception("No primary key defined for the table.");
                var parameters = new DynamicParameters();
                if (keyFields.Count == 1)
                {
                    parameters.Add("@" + keyFields[0].Property, id);
                }
                else
                {
                    var idParts = id.Split('|');
                    if (idParts.Length != keyFields.Count)
                        throw new ArgumentException("Composite key values count mismatch.");
                    for (int i = 0; i < keyFields.Count; i++)
                    {
                        parameters.Add("@" + keyFields[i].Property, idParts[i]);
                    }
                }
                var whereClause = string.Join(" AND ",
                    keyFields.Select(f => $"[{f.Property}] = @{f.Property}"));
                var deleteSql = $"DELETE FROM [{tableName}] WHERE {whereClause}";
                return await _dbConnect.ExecuteAsync(deleteSql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception when deleting:", ex);
            }
        }
        public async Task<int> DeleteMultiAsync(List<string> ids, string tableName, string status, string primaryKey)
        {
            if (ids.Count <= 0) throw new ArgumentNullException(nameof(ids));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));
            try
            {
                using (var connection = new SqlConnection(_dbConnect.ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = connection.BeginTransaction())
                    {
                        var deleteQueries = new List<string>();

                        foreach (var id in ids)
                        {
                            //bool isReferenced = false;

                            //// Tìm tất cả bảng có cột item_id (ngoại trừ chính bảng item)
                            //string sqlRefs = @"
                            //                SELECT t.name, c.name
                            //                FROM sys.columns c
                            //                INNER JOIN sys.tables t ON c.object_id = t.object_id
                            //                WHERE c.name = @KeyColumn AND t.name <> @TableName";

                            //var refs = new List<(string Table, string Column)>();
                            //using (var cmd = new SqlCommand(sqlRefs, connection, transaction))
                            //{
                            //    cmd.Parameters.AddWithValue("@KeyColumn", primaryKey);
                            //    cmd.Parameters.AddWithValue("@TableName", tableName);

                            //    using (var reader = await cmd.ExecuteReaderAsync())
                            //    {
                            //        while (await reader.ReadAsync())
                            //        {
                            //            refs.Add((reader.GetString(0), reader.GetString(1)));
                            //        }
                            //    }
                            //}

                            //// Đóng reader trước, rồi mới kiểm tra từng bảng
                            //foreach (var (refTable, refColumn) in refs)
                            //{
                            //    string checkSql = $@"
                            //                    SELECT TOP 1 1 
                            //                    FROM [{refTable}] 
                            //                    WHERE [{refColumn}] = @Id";

                            //    using (var checkCmd = new SqlCommand(checkSql, connection, transaction))
                            //    {
                            //        checkCmd.Parameters.AddWithValue("@Id", id);
                            //        var result = await checkCmd.ExecuteScalarAsync();
                            //        if (result != null)
                            //        {
                            //            isReferenced = true;
                            //            break;
                            //        }
                            //    }
                            //}

                            // Nếu có liên quan → update status
                            //if (isReferenced)
                            //{
                            deleteQueries.Add(
                                $"UPDATE [{tableName}] " +
                                $"SET status = @Status " +
                                $"WHERE [{primaryKey}] = '{id}'"
                            );
                            //}
                            //else // Không có → xóa hẳn
                            //{
                            //    deleteQueries.Add(
                            //        $"DELETE FROM [{tableName}] WHERE [{primaryKey}] = '{id}'"
                            //    );
                            //}
                        }

                        // Ghép lại thành batch query
                        var deleteQuery = string.Join(";\n", deleteQueries);
                        using (var deleteCmd = new SqlCommand(deleteQuery, connection, transaction))
                        {
                            deleteCmd.Parameters.AddWithValue("@Status", status);
                            int affectedRows = await deleteCmd.ExecuteNonQueryAsync();
                            transaction.Commit();
                            return affectedRows;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception when deleting:", ex);
            }
        }
        private async Task<int> UpsertSingleTableAsync(DataTable table, SqlJsonDefination sqlDefinition, SqlConnection connection, SqlTransaction transaction, Dictionary<string, object>? userAssign, bool? overWrite)
        {
            if (sqlDefinition == null) throw new ArgumentNullException(nameof(sqlDefinition));

            var tableName = table.TableName; // Đảm bảo lấy đúng "quotationPaper$202507"
            if (string.IsNullOrEmpty(tableName))
                tableName = sqlDefinition.ExcelIntegration?.SheetName ?? sqlDefinition.Model;

            var tempTableName = $"#{tableName}_Temp";

            // Không được mở connection ở đây vì đã truyền vào
            try
            {
                string? partition = null;
                if (tableName.Contains("$"))
                {
                    var parts = tableName.Split('$');
                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        partition = parts[1];
                    }
                }
                //ConvertDataTableColumnsFromJsonSchema(table, sqlDefinition);
                var dbColumns = await GetTableColumnsAsync(tableName, connection, transaction);

                var createTempSql = await GenerateCreateTableScript(tempTableName, dbColumns);
                using (var createCmd = new SqlCommand(createTempSql, connection, transaction))
                {
                    await createCmd.ExecuteNonQueryAsync();
                }
                //Gán giá trị userAssign và status nếu tồn tại trong DB
                var dbColumnNames = dbColumns
                    .Select(c => c.ColumnName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var defaultValues = new Dictionary<string, object>();

                if (dbColumnNames.Count > 0)
                {
                    // Duyệt qua tất cả cột trong DB
                    foreach (var col in dbColumnNames)
                    {
                        if (userAssign != null && userAssign.TryGetValue(col, out var val))
                        {
                            // Nếu FE có gửi giá trị
                            defaultValues[col] = val ?? "";
                        }
                        else
                        {
                            // Gán giá trị mặc định cho một số cột đặc biệt
                            if (col.Equals("datetime0", StringComparison.OrdinalIgnoreCase))
                                defaultValues[col] = DateTime.Now;
                            else if (col.Equals("status", StringComparison.OrdinalIgnoreCase))
                                defaultValues[col] = 1;
                            else
                                defaultValues[col] = ""; // Mặc định rỗng
                        }
                    }
                }
                foreach (var colName in defaultValues.Keys)
                {
                    if (dbColumnNames.Contains(colName))
                    {
                        // Thêm cột nếu chưa có trong DataTable
                        if (!table.Columns.Contains(colName))
                        {
                            var colType = defaultValues[colName]?.GetType() ?? typeof(object);
                            table.Columns.Add(colName, colType);
                        }

                        // Gán giá trị
                        foreach (DataRow row in table.Rows)
                        {
                            if (row[colName] == DBNull.Value || string.IsNullOrWhiteSpace(row[colName]?.ToString()))
                            {
                                row[colName] = defaultValues[colName];
                            }
                        }
                    }
                }

                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    if (!table.Columns.Contains("RowIndex"))
                    {
                        table.Columns.Add("RowIndex", typeof(int));
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            table.Rows[i]["RowIndex"] = i + 2;
                        }
                    }

                    bulkCopy.DestinationTableName = tempTableName;
                    foreach (DataColumn column in table.Columns)
                    {
                        bool hasValue = table.AsEnumerable().Any(r => r[column] != null &&
                                                                                      r[column] != DBNull.Value &&
                                                                                      !string.IsNullOrWhiteSpace(r[column].ToString()));

                        if (hasValue)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }
                    }

                    await bulkCopy.WriteToServerAsync(table);
                }
                // 1. Lấy danh sách tên cột thực tế trong bảng
                var actualColumns = table.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // 2. Ưu tiên lấy các cột required = true
                var requiredFields = sqlDefinition.Schema.Fields
                    .Where(f => f.Required == true)
                    .ToList();

                // 2. Lấy các cột thực tế có trong bảng, nhưng không bị trùng với required
                var otherFields = sqlDefinition.Schema.Fields
                    .Where(f => actualColumns.Contains(f.Property) && f.Required != true)
                    .ToList();

                // 3. Gộp lại (Required + Actual columns)
                var filteredFields = requiredFields.Concat(otherFields).ToList();

                //filter
                var filteredDefinition = new SqlJsonDefination
                {
                    Model = sqlDefinition.Model,
                    ExcelIntegration = sqlDefinition.ExcelIntegration!,
                    Schema = new SqlSchema { Fields = filteredFields },
                    Checking = sqlDefinition.Checking,
                    DataProcessing = sqlDefinition.DataProcessing,
                    ForiegnModel = sqlDefinition.ForiegnModel
                };

                // Validate dữ liệu theo rule type và sql type
                string validationSql = GenerateValidationQueryFromTempTable(tempTableName, filteredDefinition);
                var validationResults = new List<Dictionary<string, object?>>();
                if (!string.IsNullOrWhiteSpace(validationSql))
                {
                    using (var validateCmd = new SqlCommand(validationSql, connection, transaction))
                    using (var reader = await validateCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new Dictionary<string, object?>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                                row[reader.GetName(i)] = value!;
                            }
                            validationResults.Add(row);
                        }
                    }
                }
                var errors = validationResults
                    .Where(r => r["ValidationResult"]?.ToString() != "Valid")
                    .Select(r =>
                    {
                        var rowIndex = r.ContainsKey("RowIndex") && r["RowIndex"] != null
                            ? Convert.ToInt32(r["RowIndex"])
                            : -1;

                        var error = r["ValidationResult"]?.ToString() ?? "Unknown error";

                        return $"Row {rowIndex}, Error: {error}";
                    })
                    .ToList();

                //xử lý lỗi pattern check phức tạp
                foreach (var row in validationResults)
                {
                    var validation = row["ValidationResult"]?.ToString();
                    if (validation != null && validation.StartsWith("PatternCheck:"))
                    {
                        var parts = validation.Split(':');
                        if (parts.Length >= 4)
                        {
                            var fieldName = parts[1];
                            var pattern = parts[2];
                            var message = parts[3];
                            var value = row.ContainsKey(fieldName) ? row[fieldName]?.ToString() ?? "" : "";

                            if (!Regex.IsMatch(value, pattern))
                            {
                                var rowIndex = row.ContainsKey("RowIndex") && row["RowIndex"] != null
                                    ? Convert.ToInt32(row["RowIndex"])
                                    : -1;

                                errors.Add($"Row {rowIndex}, Error: {message}");
                            }
                        }
                    }
                }
                //xử lý validate bảng temp
                var (isValid, dbCheckErrors) = await ValidateDatabaseCheck(tableName, tempTableName, filteredDefinition, connection, transaction, overWrite);

                if (dbCheckErrors != null && dbCheckErrors.Count > 0)
                {
                    throw new ExceptionFormat(409, $"Upsert {tableName}", dbCheckErrors);
                }
                var allErrors = new List<string>();
                if (errors.Any()) allErrors.AddRange(errors);
                if (!isValid && dbCheckErrors != null) allErrors.AddRange(dbCheckErrors);

                if (allErrors.Any())
                {
                    throw new ExceptionFormat(null, $"Upsert {tableName}", allErrors);
                }

                //merge data
                var mergeSql = GenerateMergeSqlFromTempTable(tableName, tempTableName, filteredDefinition, dbColumnNames);

                // Thực hiện merge
                using (var mergeCmd = new SqlCommand(mergeSql, connection, transaction))
                {
                    await mergeCmd.ExecuteNonQueryAsync();
                }

                // Lấy kết quả merge
                var actions = new List<(string Action, Dictionary<string, object> Keys)>();
                using (var mergeCmd = new SqlCommand(mergeSql, connection, transaction))
                using (var reader = await mergeCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string action = reader.GetString(reader.GetOrdinal("MergeAction"));

                        // lấy toàn bộ PK động, không fix cứng "Id"
                        var keys = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("MergeAction", StringComparison.OrdinalIgnoreCase))
                                continue;

                            keys[colName] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                        }

                        actions.Add((action, keys));
                    }
                }

                // Nếu có dataprocess thì làm
                if (sqlDefinition.DataProcessing?.SqlStatements != null && actions.Any())
                {
                    // Danh sách các câu lệnh cần xử lý
                    var sqlStatements = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Insert", sqlDefinition.DataProcessing.SqlStatements.Insert },
                        { "Update", sqlDefinition.DataProcessing.SqlStatements.Update },
                        { "Delete", sqlDefinition.DataProcessing.SqlStatements.Delete }
                    };

                    foreach (var (action, keys) in actions)
                    {
                        if (!sqlStatements.TryGetValue(action, out var sqlText) || string.IsNullOrWhiteSpace(sqlText))
                            continue;
                        // bỏ qua nếu không có sql tương ứng

                        // lọc DataTable theo tất cả PK trong keys
                        var rows = table.AsEnumerable().Where(r =>
                                                            keys.All(k =>
                                                            {
                                                                var cellValue = r[k.Key];
                                                                var keyValue = k.Value;

                                                                if (cellValue == DBNull.Value && keyValue == null) return true;
                                                                if (cellValue == DBNull.Value || keyValue == null) return false;

                                                                return cellValue.ToString().Trim() == keyValue.ToString().Trim();
                                                            })
                                                        ).ToList();

                        foreach (var row in rows)
                        {
                            string realSql = sqlText;
                            if (sqlDefinition.Schema.Partition == true && partition != null)
                            {
                                // Chỉ thay trong object name, không add parameter
                                realSql = realSql.Replace("@partition", $"${partition}");
                            }
                            using (var cmd = new SqlCommand(realSql, connection, transaction))
                            {
                                var paramMatches = System.Text.RegularExpressions.Regex.Matches(sqlText, @"@\w+");
                                var addedParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                                //// Nếu có partition
                                //if (sqlDefinition.Schema.Partition == true && partition != null)
                                //{
                                //    // Chỉ thay trong object name, không add parameter
                                //    realSql = realSql.Replace("@partition", $"${partition}");
                                //}

                                foreach (Match match in paramMatches)
                                {
                                    string paramName = match.Value; // "@colName"
                                    if (addedParams.Contains(paramName)) continue;
                                    addedParams.Add(paramName);

                                    string colName = paramName.TrimStart('@');
                                    object value = table.Columns.Contains(colName)
                                        ? row[colName] ?? DBNull.Value
                                        : DBNull.Value;

                                    cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                                }

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                return table.Rows.Count;
            }
            catch (Exception ex)
            {
                if (ex is ExceptionFormat valEx)
                    throw;

                var summaries = new List<string>();

                if (ex is SqlException sqlEx)
                {
                    var msg = sqlEx.Message;
                    if (msg.Contains("Cannot insert the value NULL", StringComparison.OrdinalIgnoreCase))
                        summaries.Add($"Thiếu dữ liệu ở cột '{Regex.Match(msg, @"'([^']+)'").Groups[1].Value}'");
                    else if (msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                        summaries.Add($"Dữ liệu bị trùng khóa");
                    else
                        summaries.Add(msg);
                }
                else
                {
                    summaries.Add(ex.Message);
                }

                throw new ExceptionFormat(-1, $"Lỗi khi upsert {tableName}", summaries);
            }
        }
        private List<ItemEntry> _itemDictionary = new List<ItemEntry>();
        private void LoadItemDictionaryFromDb()
        {
            const string query = "select item_id, item_name from item where status = '1'";

            using (var connection = new SqlConnection(_dbConnect.ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    _itemDictionary.Clear();

                    while (reader.Read())
                    {
                        _itemDictionary.Add(new ItemEntry
                        {
                            ItemCode = reader["item_id"].ToString(),
                            ItemDesc = reader["item_name"].ToString()
                        });
                    }
                }
            }
        }


        // Inner helper: dò mã vật tư từ tên
        void MatchItemCodes(DataTable table, List<ItemEntry> dict)
        {
            if (!table.Columns.Contains("itemNameCustomer") || !table.Columns.Contains("itemCode")) return;

            foreach (DataRow row in table.Rows)
            {
                var input = row["itemNameCustomer"]?.ToString();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var normalized = Normalize(input);
                var best = dict
                    .Select(i => new
                    {
                        i.ItemCode,
                        i.ItemDesc,
                        Score = Fuzz.Ratio(Normalize(i.ItemDesc), normalized)
                    })
                    .OrderByDescending(x => x.Score)
                    .FirstOrDefault();

                if (best != null && best.Score > 60)
                    row["itemCode"] = best.ItemCode;
            }
        }

        string Normalize(string s)
        {
            s = s.ToLowerInvariant();
            s = Regex.Replace(s, @"[^a-z0-9\s]", " ");
            s = Regex.Replace(s, @"\s+", " ").Trim();
            return s;
        }
        public async Task MatchItemCodesFromApiAsync(DataTable table)
        {
            if (!table.Columns.Contains("itemNameCustomer") ||
                !table.Columns.Contains("itemCode") ||
                !table.Columns.Contains("nha_san_xuat"))
                return;

            // 1. Lấy URL từ cấu hình DB
            string apiUrl = await GetApiUrlFromDbAsync(_dbConnect.ConnectionString);
            if (string.IsNullOrEmpty(apiUrl)) return;

            // 2. Tạo danh sách fullname = nha_san_xuat + itemNameCustomer
            var fullNameMap = table.Rows
                .Cast<DataRow>()
                .Select(r => new
                {
                    Row = r,
                    FullName = $"{r["nha_san_xuat"]?.ToString()?.Trim()} {r["itemNameCustomer"]?.ToString()?.Trim()}".Trim()
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.FullName))
                .GroupBy(x => x.FullName) // để tránh trùng key
                .ToDictionary(g => g.Key, g => g.Select(x => x.Row).ToList());

            if (fullNameMap.Count == 0) return;

            var payload = new
            {
                names = fullNameMap.Keys.ToList()
            };

            log.Info(System.Text.Json.JsonSerializer.Serialize(payload));
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(1800);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode) return;

                var responseString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ItemMatchResult>>>(responseString);

                if (result != null && result.TryGetValue("results", out var resultsDict))
                {
                    foreach (var fullName in fullNameMap.Keys)
                    {
                        if (resultsDict.TryGetValue(fullName, out var match) && match != null)
                        {
                            if (match.TryGetValue("ItemID", out var itemId))
                            {
                                foreach (var row in fullNameMap[fullName])
                                {
                                    row["itemCode"] = itemId?.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
            }
        }


        // Lấy URL từ DB
        private async Task<string> GetApiUrlFromDbAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("SELECT ConfigValue FROM SystemOptions WHERE ConfigKey = 'url_search'", connection);
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString();
        }

        // Lớp ánh xạ kết quả
        public class ItemMatchResult : Dictionary<string, object> { }

        public async Task<int> UpsertAsyncQuaquotationPaper(Dictionary<string, DataTable> dataTables, SqlJsonDefination masterDef, List<SqlJsonDefination>? foreignDefs, Dictionary<string, object>? userAssign, bool? overWrite)
        {
            if (masterDef == null) throw new ArgumentNullException(nameof(masterDef));

            int totalRows = 0;

            using (var connection = new SqlConnection(_dbConnect.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Master
                        foreach (var kv in dataTables)
                        {
                            if (IsMatchingTable(kv.Key, masterDef.Model))
                            {
                                kv.Value.TableName = kv.Key;

                                totalRows += await UpsertSingleTableAsync(kv.Value, masterDef, connection, transaction, userAssign, overWrite);
                            }
                        }

                        // Foreign
                        if (foreignDefs != null && foreignDefs.Count > 0)
                        {
                            foreach (var foreignDef in foreignDefs)
                            {
                                foreach (var kv in dataTables)
                                {
                                    if (IsMatchingTable(kv.Key, foreignDef.Model))
                                    {
                                        kv.Value.TableName = kv.Key;
                                        // 🆕 THÊM ĐÂY: xử lý tìm mã vật tư trước khi upsert
                                        LoadItemDictionaryFromDb();
                                        //MatchItemCodes(kv.Value, _itemDictionary);
                                        await  MatchItemCodesFromApiAsync(kv.Value);
                                        totalRows += await UpsertSingleTableAsync(kv.Value, foreignDef, connection, transaction, userAssign, overWrite);
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return totalRows;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        if (ex is ExceptionFormat) throw; // Giữ nguyên lỗi validation ném lên

                        throw new ExceptionFormat(-1, "Error when upsert multiple tables", new List<string> { ex.Message });
                    }
                }
            }

        }
        public async Task<int> UpsertMultipleTablesAsync(Dictionary<string, DataTable> dataTables, SqlJsonDefination masterDef, List<SqlJsonDefination>? foreignDefs, Dictionary<string, object>? userAssign, bool? overWrite)
        {
            if (masterDef == null) throw new ArgumentNullException(nameof(masterDef));

            int totalRows = 0;

            using (var connection = new SqlConnection(_dbConnect.ConnectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Master
                        foreach (var kv in dataTables)
                        {
                            if (IsMatchingTable(kv.Key, masterDef.Model))
                            {
                                kv.Value.TableName = kv.Key;
                                totalRows += await UpsertSingleTableAsync(kv.Value, masterDef, connection, transaction, userAssign, overWrite);
                            }
                        }

                        // Foreign
                        if (foreignDefs != null && foreignDefs.Count > 0)
                        {
                            foreach (var foreignDef in foreignDefs)
                            {
                                foreach (var kv in dataTables)
                                {
                                    if (IsMatchingTable(kv.Key, foreignDef.Model))
                                    {
                                        kv.Value.TableName = kv.Key;
                                        totalRows += await UpsertSingleTableAsync(kv.Value, foreignDef, connection, transaction, userAssign, overWrite);
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return totalRows;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        if (ex is ExceptionFormat) throw; // Giữ nguyên lỗi validation ném lên

                        throw new ExceptionFormat(-1, "Error when upsert multiple tables", new List<string> { ex.Message });
                    }
                }
            }

        }
        bool IsMatchingTable(string tableKey, string model)
        {
            var prefix = tableKey.Contains('$') ? tableKey.Split('$')[0] : tableKey;
            return string.Equals(prefix, model, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// hàm này để convert lại datatable theo def và ép iểu cho đúng 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="def"></param>
        public void ConvertDataTableColumnsFromJsonSchema(DataTable table, SqlJsonDefination def)
        {
            foreach (var field in def.Schema.Fields)
            {
                var columnName = field.Property;

                if (!table.Columns.Contains(columnName)) continue;

                var targetType = GetClrTypeFromJson(field.Type);

                // Nếu đã đúng type thì bỏ qua
                if (table.Columns[columnName] != null && table.Columns[columnName]!.DataType == targetType) continue;

                // Tạo cột mới tạm
                var tempCol = new DataColumn(columnName + "_temp", targetType);
                table.Columns.Add(tempCol);

                foreach (DataRow row in table.Rows)
                {
                    var value = row[columnName];

                    if (value == DBNull.Value || string.IsNullOrWhiteSpace(value?.ToString()))
                    {
                        row[tempCol] = DBNull.Value;
                    }
                    else
                    {
                        try
                        {
                            if (targetType == typeof(DateTime))
                            {
                                var formats = new[]
                                                    {
                                                        "dd/MM/yyyy",
                                                        "dd/MM/yyyy HH:mm:ss",
                                                        "yyyy-MM-dd",
                                                        "MM/dd/yyyy",
                                                        "M/d/yy",
                                                        "M/d/yyyy",
                                                        "M/d/yy HH:mm:ss"
                                                    };

                                if (DateTime.TryParseExact(value.ToString(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                                    row[tempCol] = dt;
                                else
                                    row[tempCol] = DBNull.Value;
                            }
                            else
                            {
                                row[tempCol] = Convert.ChangeType(value, targetType);
                            }
                        }
                        catch
                        {
                            // Nếu lỗi chuyển đổi thì cho null
                            row[tempCol] = DBNull.Value;
                        }
                    }
                }

                // Xóa cột cũ, đổi tên cột mới
                table.Columns.Remove(columnName);
                tempCol.ColumnName = columnName;
            }
        }
        private Type GetClrTypeFromJson(string jsonType)
        {
            return jsonType switch
            {
                "string" => typeof(string),
                "number" => typeof(int),           // Nếu cần chính xác hơn, có thể để là decimal hoặc double nếu sqlType là DECIMAL
                "decimal" => typeof(decimal),
                "datetime" => typeof(DateTime),
                "date" => typeof(DateTime),
                "bool" => typeof(bool),
                _ => typeof(string) // fallback
            };
        }

        #region Get template
        /// <summary>
        /// Hàm đọc path json dựa theo table name 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<SqlJsonDefination> GetTemplateByTableNameAsync(string tableName)
        {
            // Ưu tiên đọc từ thư mục source code (dùng khi dev)
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "SqlJsonDefinations", "JsonModels"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlJsonDefinations", "JsonModels"),
                Path.Combine(AppContext.BaseDirectory, "SqlJsonDefinations", "JsonModels") // fallback cho trường hợp publish
            };

            string? folderPath = possiblePaths.FirstOrDefault(Directory.Exists);

            if (folderPath == null)
                throw new DirectoryNotFoundException("Cannot locate SqlJsonDefinations\\JsonModels in any expected path.");

            var expectedFileName = $"{tableName}Model.json";

            // Tìm file bất kể hoa thường
            var matchedFile = Directory.GetFiles(folderPath)
                .FirstOrDefault(f => string.Equals(Path.GetFileName(f), expectedFileName, StringComparison.OrdinalIgnoreCase));

            if (matchedFile == null)
                throw new FileNotFoundException($"JSON file not found: '{expectedFileName}' in folder {folderPath}");

            var jsonContent = await File.ReadAllTextAsync(matchedFile);
            var template = JsonConvert.DeserializeObject<SqlJsonDefination>(jsonContent);

            if (template == null)
                throw new InvalidDataException($"Cannot deserialize JSON: {matchedFile}");

            return template;
        }

        /// <summary>
        /// Hàm đọc path json metadata dựa theo table name 
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task<MetadataResponse> GetTemplateMetadataByTableNameAsync(string tableName)
        {
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Form", $"{tableName}.json"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Controllers", "Form", $"{tableName}.json"),
                Path.Combine(AppContext.BaseDirectory, "Controllers", "Form", $"{tableName}.json") // fallback cho publish
            };

            string? jsonPath = possiblePaths.FirstOrDefault(File.Exists);

            if (jsonPath == null)
                throw new FileNotFoundException($"File JSON for '{tableName}' does not exist in any expected path.");

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var template = JsonConvert.DeserializeObject<MetadataResponse>(jsonContent);

            if (template == null)
                throw new InvalidDataException($"Cannot deserialize JSON file: {jsonPath}");

            return template;
        }

        /// <summary>
        /// Hàm lấy toàn bộ định nghĩa JSON schema cho tất cả các bảng.
        /// </summary>
        /// <returns>Danh sách SqlJsonDefination</returns>
        /// <exception cref="DirectoryNotFoundException">Nếu không tìm thấy thư mục schema</exception>
        public async Task<List<SqlJsonDefination>> GetAllTemplateDefinitionsAsync()
        {
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "SqlJsonDefinations", "JsonModels"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlJsonDefinations", "JsonModels"),
                Path.Combine(AppContext.BaseDirectory, "SqlJsonDefinations", "JsonModels") // fallback cho publish
            };

            string? folderPath = possiblePaths.FirstOrDefault(Directory.Exists);

            if (folderPath == null)
                throw new DirectoryNotFoundException("Không tìm thấy thư mục SqlJsonDefinations/JsonModels trong các vị trí được kiểm tra.");

            var results = new List<SqlJsonDefination>();
            var files = Directory.GetFiles(folderPath, "*Model.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var def = JsonConvert.DeserializeObject<SqlJsonDefination>(json);
                    if (def != null)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        def.Model = fileName.Replace("Model", "");
                        results.Add(def);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi đọc file {file}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Hàm lấy primarykey 
        /// </summary>
        /// <returns>Tên primary key</returns>
        private List<string> GetPrimaryKeys(SqlJsonDefination schema)
        {
            return schema.Schema.Fields
                .Where(f => f.PrimaryKey == true)
                .Select(f => f.Name)
                .ToList();
        }
        public async Task<List<(string Code, string? Name)>> GetForeignDataAsync(string sql)
        {
            var list = new List<(string, string?)>();

            using var connection = new SqlConnection(_dbConnect.ConnectionString); // _connectionString = config DB
            await connection.OpenAsync();

            using var cmd = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var code = reader.GetFieldValue<string>(0);
                string? name = null;
                if (reader.FieldCount > 1)
                {
                    name = reader.IsDBNull(1) ? null : reader.GetString(1);
                }
                list.Add((code, name));
            }

            return list;
        }
        #endregion
        #region Generate
        /// <summary>
        /// Hàm tạo sql để tạo table template 
        /// </summary>
        /// <param name="tempTableName"></param>
        /// <param name="sqlJsonDefination"></param>
        /// <returns></returns>
        private Task<string> GenerateCreateTableScript(
                                                        string tempTableName,
                                                        List<(string ColumnName, string DataType, int? MaxLength)> dbColumns)
        {
            var columnsDef = new List<string>();

            foreach (var col in dbColumns)
            {
                string sqlType = col.DataType;

                if (col.MaxLength.HasValue)
                {
                    if (col.MaxLength.Value > 0)
                        sqlType += $"({col.MaxLength.Value})";
                    else if (col.MaxLength.Value == -1) // MAX
                        sqlType += "(MAX)";
                }

                columnsDef.Add($"[{col.ColumnName}] {sqlType}");
            }

            // Thêm RowIndex
            columnsDef.Add("[RowIndex] INT");

            string createSql = $"CREATE TABLE {tempTableName} (\n{string.Join(",\n", columnsDef)}\n);";
            return Task.FromResult(createSql);
        }


        /// <summary>
        /// Hàm tạo câu lệnh merge từ bảng phụ sang bảng chính 
        /// </summary>
        /// <param name="tempTableName"></param>
        /// <param name="sqlJsonDefination"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string GenerateMergeSqlFromTempTable(
                                                    string tableName,
                                                    string tempTableName,
                                                    SqlJsonDefination sqlJsonDefination,
                                                    HashSet<string> dbColumns)
        {
            // Copy schema fields ra list để có thể bổ sung
            var schemaFields = sqlJsonDefination.Schema.Fields.ToList();

            // Bổ sung các cột từ dbColumns nếu schema chưa có
            foreach (var col in dbColumns)
            {
                if (!schemaFields.Any(f => f.Name.Equals(col, StringComparison.OrdinalIgnoreCase)))
                {
                    schemaFields.Add(new SqlJsonDefination.SqlSchema.Field
                    {
                        Name = col,
                        Property = col,
                    });
                }
            }

            // Chỉ giữ các field có trong dbColumns (tránh cột rác)
            var validFields = schemaFields
                .Where(f => dbColumns.Contains(f.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            // Lấy key / non-key
            var keyFields = validFields.Where(f => f.PrimaryKey == true).ToList();
            var nonKeyFields = validFields.Where(f => f.PrimaryKey != true).ToList();

            if (!keyFields.Any())
                throw new ArgumentException("Primary key field not defined in schema");

            string sourceAlias = "Source";
            string targetAlias = "Target";

            // Điều kiện ON
            string onConditions = string.Join(" AND ",
                keyFields.Select(f => $"{targetAlias}.[{f.Name}] = {sourceAlias}.[{f.Name}]"));

            // SET cho UPDATE
            string updateSet = string.Join(", ",
                nonKeyFields.Select(f => $"{targetAlias}.[{f.Name}] = {sourceAlias}.[{f.Name}]"));

            // INSERT
            string insertColumns = string.Join(", ", validFields.Select(f => $"[{f.Name}]"));
            string insertValues = string.Join(", ", validFields.Select(f => $"{sourceAlias}.[{f.Name}]"));

            // OUTPUT: chỉ $action + PK
            string outputColumns = string.Join(", ",
                keyFields.Select(f => $"inserted.[{f.Name}]"));
            return $@"
                    MERGE INTO {tableName} AS {targetAlias}
                    USING {tempTableName} AS {sourceAlias}
                    ON {onConditions}
                    WHEN MATCHED THEN
                        UPDATE SET {updateSet}
                    WHEN NOT MATCHED THEN
                        INSERT ({insertColumns})
                        VALUES ({insertValues})
                    OUTPUT $action AS MergeAction, {outputColumns}; 
                    ";
        }


        private string GenerateCreateTableScriptPartition(string tableName, SqlJsonDefination sqlDef)
        {
            var columns = sqlDef.Schema.Fields.Select(f =>
                $"[{f.Name}] {f.SqlType} {(f.PrimaryKey.GetValueOrDefault(false) ? "NOT NULL" : "NULL")}"
            );

            return $@"
                    CREATE TABLE {tableName} (
                        {string.Join(",\n", columns)}
                             );";
        }

        private async Task<bool> TableExistsAsync(string tableName, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = $@"
                        SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                        WHERE TABLE_NAME = @TableName";

            using (var cmd = new SqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName.Contains('.') ? tableName.Split('.').Last() : tableName);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
        }


        /// <summary>
        /// Hàm này để tạo ra câu lệnh check validate theo type 
        /// </summary>
        /// <param name="tempTableName"></param>
        /// <param name="sqlDefination"></param>
        /// <returns></returns>
        private string GenerateValidationQueryFromTempTable(string tempTableName, SqlJsonDefination sqlDefination)
        {
            var caseWhenClauses = new List<string>();
            // Chỉ lấy các field có Required = true
            var requiredFields = sqlDefination.Schema.Fields
                .Where(f => f.Required == true)
                .ToList();
            //kiểm tra checking rule type
            foreach (var rule in sqlDefination.Checking.Rules.Where(
                                               r => requiredFields.Any(f => f.Name.Equals(r.FieldName, StringComparison.OrdinalIgnoreCase))))
            {
                string field = rule.FieldName;
                string message = rule.Message;
                string condition = "";

                switch (rule.Type)
                {
                    case "range":
                        condition = $"[{field}] < {rule.Min} OR [{field}] > {rule.Max}";
                        break;

                    case "length":
                        if (rule.MinLength != null)
                            caseWhenClauses.Add($"WHEN LEN([{field}]) < {rule.MinLength} THEN '{message}'");
                        if (rule.MaxLength != null)
                            caseWhenClauses.Add($"WHEN LEN([{field}]) > {rule.MaxLength} THEN '{message}'");
                        break;

                    case "pattern":
                        // Đánh dấu để xử lý pattern ở backend (vì SQL không hỗ trợ regex)
                        caseWhenClauses.Add($"WHEN [{field}] IS NOT NULL THEN 'PatternCheck:{field}:{rule.Pattern}:{message}'");
                        break;
                }

                if (!string.IsNullOrWhiteSpace(condition))
                {
                    caseWhenClauses.Add($"WHEN {condition} THEN '{message}'");
                }
            }

            //kiểm tra sqlType
            foreach (var field in requiredFields)
            {
                string fieldName = field.Name;
                string sqlType = field.SqlType?.ToLower() ?? "";

                if (sqlType.ToLower().Contains("decimal") || sqlType.ToLower().Contains("float") || sqlType.ToLower().Contains("numeric"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS FLOAT) IS NULL THEN 'Field {fieldName} must be a number'");
                }
                else if (sqlType.ToLower().Contains("datetime") || sqlType.ToLower().Contains("date") || sqlType.ToLower().Contains("smalldatetime"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS DATETIME) IS NULL THEN 'Field {fieldName} must be a valid date'");
                }
                else if (sqlType.ToLower().Contains("int") && !sqlType.ToLower().Contains("point") && !sqlType.ToLower().Contains("float"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS INT) IS NULL THEN 'Field {fieldName} must be an integer'");
                }
                else if (sqlType.ToLower().Contains("bit"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS BIT) IS NULL THEN 'Field {fieldName} must be a boolean'");
                }
                else if (sqlType.ToLower().Contains("uniqueidentifier"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS UNIQUEIDENTIFIER) IS NULL THEN 'Field {fieldName} must be a valid GUID'");
                }
                // Cảnh báo cho kiểu TEXT/NTEXT
                else if (sqlType.Contains("text"))
                {
                    caseWhenClauses.Add($"WHEN [{fieldName}] IS NULL THEN 'Field {fieldName} (text) should not be null'");
                }
                // Cảnh báo kiểu BINARY / IMAGE
                else if (sqlType.Contains("binary") || sqlType.Contains("image"))
                {
                    caseWhenClauses.Add($"WHEN [{fieldName}] IS NULL THEN 'Field {fieldName} (binary) should not be null'");
                }
                // XML kiểu mới
                else if (sqlType.Contains("xml"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS XML) IS NULL THEN 'Field {fieldName} must be valid XML'");
                }
                // TIME
                else if (sqlType.Contains("time"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS TIME) IS NULL THEN 'Field {fieldName} must be a valid time'");
                }
                // MONEY
                else if (sqlType.Contains("money"))
                {
                    caseWhenClauses.Add($"WHEN TRY_CAST([{fieldName}] AS MONEY) IS NULL THEN 'Field {fieldName} must be a valid money value'");
                }
                //độ dài cố định của char 
                var charMatch = Regex.Match(sqlType, @"char\((\d+)\)");
                if (charMatch.Success && int.TryParse(charMatch.Groups[1].Value, out int charMaxLen))
                {
                    caseWhenClauses.Add($"WHEN LEN([{fieldName}]) > {charMaxLen} THEN 'Field {fieldName} exceeds max length {charMaxLen}'");
                }
                // Kiểm tra kiểu dữ liệu TEXT, NTEXT, IMAGE
            }

            string caseStatement = string.Join("\n", caseWhenClauses);
            if (string.IsNullOrWhiteSpace(caseStatement))
            {
                return "";
            }
            return $@"
					SELECT *,
					CASE
						{caseStatement}
					ELSE 'Valid'
					END AS ValidationResult
					FROM {tempTableName}
					";
        }

        /// <summary>
        /// Hàm tạo câu lệnh query sử dụng open json 
        /// </summary>
        /// <param name="sqlDefination"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string GenerateValidationQueryByOpenJson(SqlJsonDefination sqlDefination)
        {
            var caseWhenClauses = new List<string>();
            var fields = new List<string>();
            foreach (var rule in sqlDefination.Checking.Rules)
            {
                string field = rule.FieldName;
                string message = rule.Message;
                string condition = "";
                switch (rule.Type)
                {
                    case "range":
                        condition = $"[{field}] < {rule.Min} OR [{field}] > {rule.Max}";
                        break;
                    case "length":
                        if (rule.MinLength != null) caseWhenClauses.Add($"WHEN LEN([{field}]) < {rule.MinLength} THEN '{message}'");
                        if (rule.MaxLength != null) caseWhenClauses.Add($"WHEN LEN([{field}]) > {rule.MaxLength} THEN '{message}'");
                        break;
                    case "pattern":
                        // Dùng PATINDEX thay vì LIKE
                        var pattern = rule.Pattern?.Replace("'", "''") ?? "";
                        condition = $"PATINDEX('{pattern}', [{field}]) = 0";
                        break;
                }
                if (!string.IsNullOrWhiteSpace(condition))
                {
                    caseWhenClauses.Add($"WHEN {condition} THEN '{message}'");
                }
            }
            foreach (var field in sqlDefination.Schema.Fields)
            {
                var sqlDef = sqlDefination.Schema.Fields.FirstOrDefault(t => t.Name.Trim().ToLower().Equals(field.Name.Trim().ToLower()));
                if (sqlDef == null)
                {
                    throw new Exception($"Field '{field}' not exist in schema.");
                }
                string typeField = sqlDef.SqlType ?? "";
                //Chuyển kiểu không được hỗ trợ sang kiểu hợp lệ
                typeField = typeField switch
                {
                    "TEXT" => "NVARCHAR(MAX)",
                    "NTEXT" => "NVARCHAR(MAX)",
                    "IMAGE" => "VARBINARY(MAX)",
                    "SQL_VARIANT" => throw new Exception($"SQL_VARIANT is not supported in OPENJSON WITH clause. Field: {field.Name}"),
                    _ => typeField
                };
                fields.Add($"{field.Name} {typeField}");
            }
            string cases = string.Join("\n", caseWhenClauses);
            string casesField = string.Join(",\n", fields);

            return $@"
				SELECT *,
				CASE
					{cases}
					ELSE 'Valid'
				END AS ValidationResult
				FROM OPENJSON(@json)
				WITH (
					{casesField}
				)
			";
        }

        private async Task<List<(string ColumnName, string DataType, int? MaxLength)>> GetTableColumnsAsync(
                                                                                                            string tableName,
                                                                                                            SqlConnection connection,
                                                                                                            SqlTransaction transaction)
        {
            var columns = new List<(string ColumnName, string DataType, int? MaxLength)>();

            string sql = @"
                        SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = @TableName
                        ORDER BY ORDINAL_POSITION";

            using (var cmd = new SqlCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string columnName = reader.GetString(0);
                        string dataType = reader.GetString(1);
                        int? maxLength = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);

                        columns.Add((columnName, dataType, maxLength));
                    }
                }
            }

            return columns;
        }

        #endregion
        #region Validate
        /// <summary>
        /// Hàm xử lý validate chung open json 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        private async Task ValidateEntityAsync(object entity, SqlJsonDefination def)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (def == null) throw new ArgumentNullException(nameof(def));

            var json = JsonConvert.SerializeObject(entity);
            var parameters = new DynamicParameters();
            parameters.Add("@json", json);

            string sqlValidation = GenerateValidationQueryByOpenJson(def);
            var validationResults = await _dbConnect.QueryAsync<dynamic>(sqlValidation, parameters);

            var errors = validationResults
                .Where(r => r.ValidationResult != "Valid")
                .Select(r => $"Row: {r}, Error: {r.ValidationResult}")
                .ToList();

            if (errors.Count > 0)
                throw new Exception("Validation error(s):\n" + string.Join("\n", errors));
        }

        /// <summary>
        /// Hàm kiểm tra database đã tồn tại chưa trong temp 
        /// </summary>
        /// <param name="oldTable"></param>
        /// <param name="sqlDefination"></param>
        /// <returns></returns>
        private async Task<(bool isValid, List<string>? errors)> ValidateDatabaseCheck(
                                                                                        string tableName,
                                                                                        string tempTableName,
                                                                                        SqlJsonDefination sqlDefination,
                                                                                        SqlConnection connection,
                                                                                        SqlTransaction transaction,
                                                                                        bool? overWrite)
        {
            var errors = new List<string>();
            int rowIndex = 1;

            try
            {
                var dbChecks = sqlDefination.Checking.Rules
                    .Where(f => f.Type?.ToLower() == "databasecheck")
                    .ToList();

                if (!dbChecks.Any())
                    return (true, errors);

                foreach (var field in dbChecks)
                {
                    try
                    {
                        //nếu user cho phép overwrite và json có overwrite thì tiếp tục 
                        if (overWrite == true && field.OverWrite == true)
                        {
                            continue;
                        }
                        string fieldName = field.FieldName!;
                        string message = field.Message;
                        int threshold = int.Parse(field.Threshold ?? "0");
                        string whereClause = field.CheckQuery!.Trim();

                        // Bỏ "WHERE" nếu có
                        if (whereClause.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
                            whereClause = whereClause.Substring(5).Trim();

                        // Tìm tất cả các @param
                        var paramMatches = Regex.Matches(whereClause, @"@(\w+)");
                        var joinConditions = new List<string>();

                        foreach (Match match in paramMatches)
                        {
                            var cleanName = match.Groups[1].Value;
                            // Thay @param bằng t.[field]
                            whereClause = Regex.Replace(whereClause, $@"\B{Regex.Escape(match.Value)}\b", $"t.[{cleanName}]");
                            //thay bên trái = sang m.
                            whereClause = Regex.Replace(whereClause, $@"(?<!\.)(\b{cleanName}\b)(?!\])", $"m.[{cleanName}]");
                            joinConditions.Add($"t.[{cleanName}] = m.[{cleanName}]");
                        }

                        // Nếu chỉ có 1 param thì joinConditions sẽ có 1 phần tử, nhiều param thì joinConditions nhiều phần tử
                        string onCondition = string.Join(" AND ", joinConditions);

                        // Build câu query
                        string bulkCheckQuery = $@"
                            SELECT {string.Join(", ", paramMatches.Select(m => $"t.[{m.Groups[1].Value}]"))}, COUNT(*) AS Total
                            FROM {tableName ?? "TargetTable"} m
                            JOIN {tempTableName} t ON {onCondition}
                            WHERE {whereClause}
                            GROUP BY {string.Join(", ", paramMatches.Select(m => $"t.[{m.Groups[1].Value}]"))}
                            HAVING COUNT(*) > {threshold}
                        ";


                        using var checkCmd = new SqlCommand(bulkCheckQuery, connection, transaction);
                        using var reader = await checkCmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            var value = reader.GetValue(0);
                            var count = reader.GetInt32(reader.GetOrdinal("Total"));
                            rowIndex++;
                            errors.Add($"[Dòng {rowIndex}]: {message} tại giá trị '{value}' (số lần = {count})");
                        }

                        await reader.CloseAsync();

                        // Kiểm tra trùng trong bảng temp
                        var groupColumns = paramMatches.Select(m => $"[{m.Groups[1].Value}]").ToList();

                        string duplicateInTempQuery = $@"
                        SELECT {string.Join(", ", groupColumns)}, COUNT(*) AS Total
                        FROM {tempTableName}
                        GROUP BY {string.Join(", ", groupColumns)}
                        HAVING COUNT(*) > 1";

                        using var tempCheckCmd = new SqlCommand(duplicateInTempQuery, connection, transaction);
                        using var tempReader = await tempCheckCmd.ExecuteReaderAsync();

                        while (await tempReader.ReadAsync())
                        {
                            var value = tempReader.GetValue(0);
                            var count = tempReader.GetInt32(1);
                            rowIndex++;
                            errors.Add($"[Dòng {rowIndex}]: {fieldName} bị trùng tại giá trị '{value}' (số lần = {count})");
                        }

                        await tempReader.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"[DatabaseCheck Error]: Field '{field.FieldName}' - {ex.Message}");
                    }
                }

                //kiểm tra external
                var exChecks = sqlDefination.Checking.Rules.Where(f => f.Type?.ToLower() == "queryCheck").ToList();

                if (!exChecks.Any())
                    return (true, errors);

                foreach (var field in exChecks)
                {
                    try
                    {
                        var tableExName = field.TableName;
                        string message = field.Message;
                        int threshold = int.Parse(field.Threshold ?? "0");
                        string whereClause = field.CheckQuery!.Trim();

                        // Bỏ "WHERE" nếu có
                        if (whereClause.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase))
                            whereClause = whereClause.Substring(5).Trim();
                        
                        // Tìm tất cả các @param
                        var paramMatches = Regex.Matches(whereClause, @"@(\w+)");
                        var joinConditions = new List<string>();

                        // Sau khi replace @param thành t.[field]
                        foreach (Match match in paramMatches)
                        {
                            var cleanName = match.Groups[1].Value;

                            // replace @param → t.[cleanName]
                            whereClause = Regex.Replace(whereClause, $@"\B{Regex.Escape(match.Value)}\b", $"t.[{cleanName}]");

                            // ép cột bên external thành m.[...] nếu không có alias
                            whereClause = Regex.Replace(whereClause, $@"\b(\w+)\b(?=\s*=\s*t\.\[{cleanName}\])", "m.$1");

                            joinConditions.Add($"t.[{cleanName}] = m.[{cleanName}]");
                        }

                        // Nếu chỉ có 1 param thì joinConditions sẽ có 1 phần tử, nhiều param thì joinConditions nhiều phần tử
                        string onCondition = string.Join(" AND ", joinConditions);

                        // Build câu query
                        string bulkCheckQuery = $@"
                            SELECT {string.Join(", ", paramMatches.Select(m => $"t.[{m.Groups[1].Value}]"))}, COUNT(*) AS Total
                            FROM {tableExName ?? "TargetTable"} m
                            JOIN {tempTableName} t ON {onCondition}
                            WHERE {whereClause}
                            GROUP BY {string.Join(", ", paramMatches.Select(m => $"t.[{m.Groups[1].Value}]"))}
                            HAVING COUNT(*) > {threshold}
                        ";

                        using var checkCmd = new SqlCommand(bulkCheckQuery, connection, transaction);
                        using var reader = await checkCmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            var value = reader.GetValue(0);
                            var count = reader.GetInt32(reader.GetOrdinal("Total"));
                            rowIndex++;
                            errors.Add($"[Dòng {rowIndex}]: {message} tại giá trị '{value}' (số lần = {count})");
                        }

                        await reader.CloseAsync();
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"[QueryCheck Error]: Field '{field.FieldName}' - {ex.Message}");
                    }
                }

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"[ValidateDatabaseCheck Fatal Error]: {ex.Message}");
                return (false, errors);
            }
        }

        #endregion
        #region external function
        ///hàm kiểm tra và đưa ra new table dựa theo các field tương ứng với json map được
        private List<string> CheckMissingRequiredData(DataTable dataTable, SqlJsonDefination.ExcelIntegrationMap excelColumn)
        {
            var errors = new List<string>();

            // Tạo dictionary để check required nhanh hơn
            var requiredFields = excelColumn.ColumnMapping
                .Where(x => x.Required == true)
                .ToDictionary(x => x.FieldName.Trim(), x => true, StringComparer.OrdinalIgnoreCase);

            // Check cột có thiếu không
            foreach (var field in requiredFields.Keys)
            {
                if (!dataTable.Columns.Contains(field))
                {
                    errors.Add($"Thiếu cột required: {field}");
                }
            }

            // Nếu thiếu cột, trả lỗi luôn không cần check giá trị
            if (errors.Count > 0)
                return errors;

            // Check giá trị từng dòng
            for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                var row = dataTable.Rows[rowIndex];

                foreach (var field in requiredFields.Keys)
                {
                    var value = row[field];
                    if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        errors.Add($"Cột '{field}' không có dữ liệu (required).");
                    }
                }
            }

            return errors;
        }

        public (Dictionary<string, DataTable> tables, List<string> errors) SplitFlatTableWithHeaderDetection(
                                                                                                            DataTable flatTable,
                                                                                                            SqlJsonDefination masterDef,
                                                                                                            List<SqlJsonDefination>? foreignDefs)
        {
            var result = new Dictionary<string, DataTable>();
            var errors = new List<string>();

            // Mapping bây giờ dùng List<int> để lưu tất cả index trùng
            List<(SqlJsonDefination def, Dictionary<string, List<int>> mapping)>? currentMatchedDefs = null;
            var allDefs = new List<SqlJsonDefination> { masterDef };
            if (foreignDefs != null && foreignDefs.Count > 0)
                allDefs.AddRange(foreignDefs);

            var masterKeys = new List<(string numberKey, string dateKey)>();
            var masterIdKeys = new List<(string pkValue, string numberKey)>();
            //Tìm tất cả dòng header
            var headerRowIndexes = new List<int>();
            for (int i = 0; i < flatTable.Rows.Count; i++)
            {
                var row = flatTable.Rows[i];
                // Detect header
                var matchedDefs = FindAllMatchedDefs(row, allDefs);

                if (matchedDefs.Any())
                {
                    //waitingForData = true;
                    currentMatchedDefs = matchedDefs;
                    headerRowIndexes.Add(i);
                }
            }

            // Xử lý data row
            if (currentMatchedDefs != null)
            {
                for (int i = 0; i < flatTable.Rows.Count; i++)
                {
                    if (headerRowIndexes.Contains(i))
                        continue;
                    var row = flatTable.Rows[i];
                    var idRow = "";
                    string dateKey = "";
                    bool skipAddMasterRow = false;
                    foreach (var (def, mapping) in currentMatchedDefs)
                    {
                        string? voucherNumberVal = null;
                        string? voucherDateVal = null;

                        // MASTER có Partition
                        if (def.Model == masterDef.Model && def.Schema.Partition == true)
                        {
                            // Lấy voucherNumber (master → lấy index đầu tiên)
                            if (mapping.TryGetValue("voucherNumber", out var vnIndexes) && vnIndexes.Count > 0)
                            {
                                var vnColIdx = vnIndexes.First();
                                if (vnColIdx >= 0 && vnColIdx < flatTable.Columns.Count)
                                    voucherNumberVal = row[vnColIdx]?.ToString()?.Trim();
                            }
                            // Nếu voucherNumber chưa có → fallback lấy primaryKey trong schema
                            if (string.IsNullOrWhiteSpace(voucherNumberVal))
                            {
                                // Tìm cột nào được đánh dấu primaryKey = true
                                var primaryKeyField = def.Schema.Fields.FirstOrDefault(f => f.PrimaryKey == true);
                                if (primaryKeyField != null)
                                {
                                    if (mapping.TryGetValue(primaryKeyField.Name, out var pkIndexes) && pkIndexes.Count > 0)
                                    {
                                        var pkColIdx = pkIndexes.First();
                                        if (pkColIdx >= 0 && pkColIdx < flatTable.Columns.Count)
                                            voucherNumberVal = row[pkColIdx]?.ToString()?.Trim();
                                    }
                                }
                            }
                            // Kiểm tra tồn tại trong masterKeys
                            var existedMaster = !string.IsNullOrWhiteSpace(voucherNumberVal)
                                ? masterKeys.FirstOrDefault(x =>
                                    x.numberKey.Equals(voucherNumberVal, StringComparison.OrdinalIgnoreCase))
                                : default;

                            bool isMasterExisted = !string.IsNullOrWhiteSpace(existedMaster.numberKey);

                            if (isMasterExisted)
                            {
                                dateKey = existedMaster.dateKey;
                            }

                            if (string.IsNullOrWhiteSpace(voucherNumberVal))
                            {
                                isMasterExisted = true;
                                if (!string.IsNullOrWhiteSpace(dateKey))
                                {
                                    var existedByDate = masterKeys
                                        .FirstOrDefault(x => x.dateKey.Equals(dateKey, StringComparison.OrdinalIgnoreCase));

                                    if (!string.IsNullOrWhiteSpace(existedByDate.numberKey))
                                    {
                                        voucherNumberVal = existedByDate.numberKey;

                                        if (mapping.ContainsKey("voucherNumber"))
                                        {
                                            var vnColIdx = mapping["voucherNumber"].First();
                                            if (vnColIdx >= 0 && vnColIdx < flatTable.Columns.Count)
                                                row[vnColIdx] = voucherNumberVal;
                                        }
                                        else
                                        {
                                            if (!flatTable.Columns.Contains("voucherNumber"))
                                                flatTable.Columns.Add("voucherNumber", typeof(string));
                                            row["voucherNumber"] = voucherNumberVal;
                                        }
                                    }
                                }
                            }

                            // Lấy voucherDate
                            if (mapping.TryGetValue("voucherDate", out var vdIndexes) && vdIndexes.Count > 0)
                            {
                                var vdColIdx = vdIndexes.First();
                                if (vdColIdx >= 0 && vdColIdx < flatTable.Columns.Count)
                                    voucherDateVal = row[vdColIdx]?.ToString()?.Trim();
                            }

                            // Nếu rỗng → gán ngày hiện tại
                            if (string.IsNullOrWhiteSpace(voucherDateVal))
                            {
                                voucherDateVal = DateTime.Now.ToString("yyyy-MM-dd");

                                if (mapping.ContainsKey("voucherDate"))
                                {
                                    var vdColIdx = mapping["voucherDate"].First();
                                    row[vdColIdx] = voucherDateVal;
                                }
                                else
                                {
                                    if (!flatTable.Columns.Contains("voucherDate"))
                                        flatTable.Columns.Add("voucherDate", typeof(string));
                                    row["voucherDate"] = voucherDateVal;
                                }
                            }

                            // Validate
                            if (!DateTime.TryParse(voucherDateVal, out var parsedDate))
                            {
                                errors.Add($"Dòng {i + 2}: Ngày chứng từ không hợp lệ ({voucherDateVal}) trong bảng {def.Model}.");
                                continue;
                            }

                            dateKey = parsedDate.ToString("yyyyMM");

                            // Lưu master key nếu chưa tồn tại
                            if (!isMasterExisted)
                                masterKeys.Add((voucherNumberVal!, dateKey));
                            else
                                skipAddMasterRow = true;
                        }
                        else if(def.Model == masterDef.Model && def.Schema.Multiple == true)
                        {
                            // Tìm field nào được đánh dấu PrimaryKey = true
                            var primaryKeyField = def.Schema.Fields.FirstOrDefault(f => f.PrimaryKey == true);
                            if (primaryKeyField != null)
                            {
                                // Lấy giá trị khóa chính từ row
                                if (mapping.TryGetValue(primaryKeyField.Name, out var pkIndexes) && pkIndexes.Count > 0)
                                {
                                    var pkColIdx = pkIndexes.First();
                                    if (pkColIdx >= 0 && pkColIdx < flatTable.Columns.Count)
                                        voucherNumberVal = row[pkColIdx]?.ToString()?.Trim();
                                }
                            }

                            // Kiểm tra tồn tại trong masterKeys (so sánh theo numberKey = primaryKey)
                            var existedMaster = !string.IsNullOrWhiteSpace(voucherNumberVal)
                                ? masterKeys.FirstOrDefault(x =>
                                    x.numberKey.Equals(voucherNumberVal, StringComparison.OrdinalIgnoreCase))
                                : default;

                            bool isMasterExisted = !string.IsNullOrWhiteSpace(existedMaster.numberKey);

                            // Với Multiple không có dateKey → có thể set rỗng hoặc null
                            dateKey = string.Empty;

                            // Nếu chưa có thì thêm vào masterKeys
                            if (!isMasterExisted)
                                masterKeys.Add((voucherNumberVal!, dateKey));
                            else
                                skipAddMasterRow = true;
                        }
                        // DETAIL
                        else if (def.Model != masterDef.Model && foreignDefs != null)
                        {
                            foreach (var detailDef in foreignDefs)
                            {
                                if (detailDef.Model != def.Model) continue;

                                string? detailVoucherNumber = null;

                                // Ưu tiên: Detail → lấy index cuối cùng của voucherNumber
                                if (mapping.TryGetValue("voucherNumber", out var vnIndexes) && vnIndexes.Count > 0)
                                {
                                    var vnColIdx = vnIndexes.Last();
                                    if (vnColIdx >= 0 && vnColIdx < flatTable.Columns.Count)
                                        detailVoucherNumber = row[vnColIdx]?.ToString()?.Trim();
                                }

                                // Nếu detail không có voucherNumber → lấy từ master (nếu có)
                                if (string.IsNullOrWhiteSpace(detailVoucherNumber))
                                {
                                    var masterMapping = currentMatchedDefs
                                        .FirstOrDefault(x => x.def.Model == masterDef.Model)
                                        .mapping;

                                    if (masterMapping != null && masterMapping.TryGetValue("voucherNumber", out var masterVnIndexes))
                                    {
                                        var vnColIdx = masterVnIndexes.First();
                                        if (vnColIdx >= 0 && vnColIdx < flatTable.Columns.Count)
                                            detailVoucherNumber = row[vnColIdx]?.ToString()?.Trim();
                                    }
                                }

                                // Nếu master cũng không có voucherNumber → lấy PrimaryKey của master
                                if (string.IsNullOrWhiteSpace(detailVoucherNumber))
                                {
                                    var primaryKeyField = masterDef.Schema.Fields.FirstOrDefault(f => f.PrimaryKey == true);
                                    if (primaryKeyField != null)
                                    {
                                        var masterMapping = currentMatchedDefs
                                            .FirstOrDefault(x => x.def.Model == masterDef.Model)
                                            .mapping;

                                        if (masterMapping != null && masterMapping.TryGetValue(primaryKeyField.Name, out var pkIndexes) && pkIndexes.Count > 0)
                                        {
                                            var pkColIdx = pkIndexes.First();
                                            if (pkColIdx >= 0 && pkColIdx < flatTable.Columns.Count)
                                                detailVoucherNumber = row[pkColIdx]?.ToString()?.Trim();
                                        }
                                    }
                                }

                                // Cuối cùng: nếu có số chứng từ hoặc PK thì match với master
                                if (!string.IsNullOrWhiteSpace(detailVoucherNumber))
                                {
                                    var matchedMaster = masterKeys
                                        .FirstOrDefault(x =>
                                            x.numberKey.Equals(detailVoucherNumber, StringComparison.OrdinalIgnoreCase));

                                    if (!string.IsNullOrWhiteSpace(matchedMaster.numberKey))
                                    {
                                        dateKey = matchedMaster.dateKey;
                                        voucherNumberVal = detailVoucherNumber;
                                    }
                                }
                            }
                        }


                        if (skipAddMasterRow && def.Model == masterDef.Model) continue;

                        var tableKey = def.Schema.Partition == true ? $"{def.Model}${dateKey}" : $"{def.Model}";
                        if (!result.ContainsKey(tableKey))
                        {
                            var table = new DataTable();
                            foreach (var col in def.Schema.Fields)
                                table.Columns.Add(col.Name);
                            result[tableKey] = table;
                        }

                        var currentTable = result[tableKey];
                        var newRow = currentTable.NewRow();

                        bool isIdExistByVoucherNumber = false;
                        // Bước 1: đọc toàn bộ cellValue trước
                        var fieldValues = new Dictionary<string, object?>();
                        foreach (var fieldDef in def.Schema.Fields)
                        {
                            var fieldName = fieldDef.Name;
                            object? cellValue = null;

                            if (mapping.TryGetValue(fieldName, out var indexes) && indexes.Count > 0)
                            {
                                int fieldIdx = (indexes.Count > 1)
                                    ? (def.Model == masterDef.Model ? indexes.First() : indexes.Last())
                                    : indexes[0];

                                if (fieldIdx >= 0 && fieldIdx < row.ItemArray.Length)
                                    cellValue = row[fieldIdx];
                            }
                            else
                            {
                                if (flatTable.Columns.Contains(fieldName))
                                    cellValue = row[fieldName];
                                else
                                    cellValue = DBNull.Value;
                            }

                            fieldValues[fieldName] = cellValue;
                        }

                        // Bước 2: xử lý voucherNumber trước
                        string? voucherNumberStr = null;
                        if (fieldValues.TryGetValue("voucherNumber", out var vnVal) && vnVal != null)
                            voucherNumberStr = vnVal.ToString()?.Trim();

                        // Nếu có voucherNumber → ưu tiên ánh xạ masterIdKeys
                        if (!string.IsNullOrWhiteSpace(voucherNumberStr))
                        {
                            var pkName = def.Schema.Fields.FirstOrDefault(f => f.PrimaryKey == true)?.Name;
                            var dbMasterId = GetExistingIdByVoucherNumber(pkName, voucherNumberStr, tableKey);

                            if (!string.IsNullOrWhiteSpace(dbMasterId))
                            {
                                // update/insert vào masterIdKeys
                                var existingIdx = masterIdKeys.FindIndex(x =>
                                    x.numberKey.Equals(voucherNumberStr, StringComparison.OrdinalIgnoreCase));
                                if (existingIdx >= 0)
                                    masterIdKeys[existingIdx] = (dbMasterId, voucherNumberStr);
                                else
                                    masterIdKeys.Add((dbMasterId, voucherNumberStr));

                                // nếu PK hiện tại đang rỗng → gán luôn id này
                                if (pkName != null && string.IsNullOrWhiteSpace(fieldValues[pkName]?.ToString()))
                                {
                                    fieldValues[pkName] = dbMasterId;
                                    isIdExistByVoucherNumber = true;
                                }
                            }
                        }

                        foreach (var fieldDef in def.Schema.Fields)
                        {
                            var fieldName = fieldDef.Name;
                            object? cellValue = null;

                            if (mapping.TryGetValue(fieldName, out var indexes) && indexes.Count > 0)
                            {
                                int fieldIdx = (indexes.Count > 1)
                                    ? (def.Model == masterDef.Model ? indexes.First() : indexes.Last())
                                    : indexes[0];

                                if (fieldIdx >= 0 && fieldIdx < row.ItemArray.Length)
                                    cellValue = row[fieldIdx];
                            }
                            else
                            {
                                if (flatTable.Columns.Contains(fieldName))
                                    cellValue = row[fieldName];
                                else
                                    cellValue = DBNull.Value;
                            }
                            //nếu khóa chính có mà ở bảng master thì gán vào masterIdKey
                            if (fieldDef.PrimaryKey == true && !string.IsNullOrWhiteSpace(cellValue?.ToString()) && def.Model == masterDef.Model)
                            {
                                masterIdKeys.Add((cellValue!.ToString()!, cellValue!.ToString()!));
                                idRow = cellValue.ToString();
                            }

                            // Xử lý khóa chính rỗng
                            if (fieldDef.PrimaryKey == true && string.IsNullOrWhiteSpace(cellValue?.ToString()))
                            {
                                if (def.Model == masterDef.Model)
                                {
                                    if (isIdExistByVoucherNumber)
                                    {
                                        cellValue = fieldValues[fieldDef.Name];
                                    }
                                    else
                                    {
                                        var newGuid = Guid.NewGuid().ToString();

                                        if (mapping.ContainsKey(fieldName))
                                        {
                                            var idx = mapping[fieldName].First();
                                            // Tìm column mapping tương ứng trong ExcelIntegration
                                            var colMap = masterDef.ExcelIntegration.ColumnMapping
                                                .FirstOrDefault(c => c.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                                            if (colMap?.Required != true) // Chỉ tạo Guid nếu không bắt buộc
                                            {
                                                if (idx >= 0 && idx < row.ItemArray.Length)
                                                    row[idx] = newGuid;
                                            }
                                            else
                                            {
                                                throw new ExceptionFormat($"Khóa chính của bảng ({def.Model}) không được để trống");
                                            }
                                        }
                                        else
                                        {
                                            if (!flatTable.Columns.Contains(fieldName))
                                                flatTable.Columns.Add(fieldName, typeof(string));
                                            row[fieldName] = newGuid;
                                        }

                                        cellValue = newGuid;
                                        //if (!string.IsNullOrWhiteSpace(voucherNumberVal))
                                        masterIdKeys.Add((cellValue!.ToString()!, voucherNumberVal ?? cellValue!.ToString()!));
                                    }
                                }
                                else
                                {
                                    string? matchedMasterId = null;
                                    if (!string.IsNullOrWhiteSpace(voucherNumberVal))
                                    {
                                        matchedMasterId = masterIdKeys
                                            .FirstOrDefault(x =>
                                                x.numberKey.Equals(voucherNumberVal, StringComparison.OrdinalIgnoreCase)).pkValue;
                                    }
                                    else
                                    {
                                        matchedMasterId = masterIdKeys
                                           .FirstOrDefault(x =>
                                               x.numberKey.Equals(idRow, StringComparison.OrdinalIgnoreCase)).pkValue;
                                    }
                                    if (!string.IsNullOrWhiteSpace(matchedMasterId))
                                    {
                                        cellValue = matchedMasterId;

                                        if (mapping.ContainsKey(fieldName))
                                        {
                                            var idx = mapping[fieldName].First();
                                            if (idx >= 0 && idx < row.ItemArray.Length)
                                                row[idx] = matchedMasterId;
                                        }
                                        else
                                        {
                                            if (!flatTable.Columns.Contains(fieldName))
                                                flatTable.Columns.Add(fieldName, typeof(string));
                                            row[fieldName] = matchedMasterId;
                                        }
                                    }

                                }
                            }

                            newRow[fieldName] = cellValue ?? DBNull.Value;
                        }

                        // Check required data
                        var tempTable = currentTable.Clone();
                        tempTable.Rows.Add(newRow.ItemArray);
                        var missingDataErrors = CheckMissingRequiredData(tempTable, def.ExcelIntegration);
                        if (missingDataErrors.Count > 0)
                        {
                            errors.AddRange(missingDataErrors);
                            continue;
                        }

                        currentTable.Rows.Add(newRow);
                    }
                }
            }

            if (!result.Any())
                errors.Add("Không tìm thấy bảng nào phù hợp trong file.");

            return (result, errors);
        }

        private string? GetExistingIdByVoucherNumber(string? pk, string voucherNumber, string tableName)
        {
            if (string.IsNullOrWhiteSpace(pk))
                throw new ArgumentException("Primary key column name is required", nameof(pk));
            using (var connection = new SqlConnection(_dbConnect.ConnectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand($@"
                        SELECT TOP 1 {pk} 
                        FROM {tableName} 
                        WHERE voucherNumber = @voucherNumber AND status = '1'", connection))
                {
                    cmd.Parameters.AddWithValue("@voucherNumber", voucherNumber);

                    var result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : null;
                }
            }
        }

        private string GetGroupKey(DataRow row, Dictionary<string, List<int>> mapping, SqlJsonDefination def)
        {
            // 1. Nếu có voucherNumber → ưu tiên
            if (mapping.TryGetValue("voucherNumber", out var vnIndexes) && vnIndexes.Count > 0)
            {
                var val = row[vnIndexes.First()]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(val))
                    return val;
            }

            // 2. Nếu có primaryKey → dùng
            var pkField = def.Schema.Fields.FirstOrDefault(f => f.PrimaryKey == true)?.Name;
            if (!string.IsNullOrEmpty(pkField) && mapping.TryGetValue(pkField, out var pkIndexes))
            {
                var val = row[pkIndexes.First()]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(val))
                    return val;
            }

            // 3. Nếu có foriegnKey → dùng
            if (def.Schema.ForiegnKey != null)
            {
                foreach (var fk in def.Schema.ForiegnKey)
                {
                    if (mapping.TryGetValue(fk, out var fkIndexes))
                    {
                        var val = row[fkIndexes.First()]?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(val))
                            return val;
                    }
                }
            }

            // 4. Nếu có voucherDate + customer_id → dùng
            string? datePart = "";
            if (mapping.TryGetValue("voucherDate", out var vdIndexes) && vdIndexes.Count > 0)
                datePart = row[vdIndexes.First()]?.ToString()?.Trim();

            string? custPart = "";
            if (mapping.TryGetValue("customer_id", out var custIndexes) && custIndexes.Count > 0)
                custPart = row[custIndexes.First()]?.ToString()?.Trim();

            if (!string.IsNullOrWhiteSpace(datePart) || !string.IsNullOrWhiteSpace(custPart))
                return $"{datePart}|{custPart}";

            // 5. Fallback → random GUID để vẫn tách được
            return Guid.NewGuid().ToString();
        }

        private static List<(SqlJsonDefination def, Dictionary<string, List<int>> mapping)> FindAllMatchedDefs(
                                                                                            DataRow headerRow, List<SqlJsonDefination> defs, double threshold = 0.1)
        {
            var headerFields = headerRow.ItemArray.Select(x => CleanColumnName(x?.ToString() ?? "")).ToList();

            var matchedDefs = new List<(SqlJsonDefination, Dictionary<string, List<int>>)>();

            foreach (var def in defs)
            {
                // Nếu không có ExcelIntegration hoặc không có ColumnMapping thì bỏ qua luôn
                if (def?.ExcelIntegration?.ColumnMapping == null || def.ExcelIntegration.ColumnMapping.Count == 0)
                    continue;
                var fieldMap = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                int matched = 0;

                for (int i = 0; i < headerFields.Count; i++)
                {
                    var header = headerFields[i];

                    var found = def.ExcelIntegration.ColumnMapping
                        .FirstOrDefault(c => CleanColumnName(c.FieldName) == header);

                    if (found != null)
                    {
                        if (!fieldMap.ContainsKey(found.FieldName))
                            fieldMap[found.FieldName] = new List<int>();

                        fieldMap[found.FieldName].Add(i);
                        matched++;
                    }
                }

                double score = (double)matched / def.ExcelIntegration.ColumnMapping.Count;

                if (score >= threshold && matched > 0)
                {
                    matchedDefs.Add((def, fieldMap));
                }
            }

            return matchedDefs;
        }


        private static string CleanColumnName(string input)
        {
            return input.Replace("*", "").Trim().ToLower();
        }
        #endregion
    }
}
