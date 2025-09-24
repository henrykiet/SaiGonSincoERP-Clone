using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using reportSystem01.Server.Models;
using ReportServer.Services;
using Microsoft.Extensions.Configuration;
using reportSystem01.Shared;
using static reportSystem01.Shared.ReportFilter;

namespace ReportServer.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;
        
        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        
        }
        public async Task<List<Dictionary<string, object>>> GetReportsAsync(int user)
        {
            var reports = new List<Dictionary<string, object>>();  // Danh sách kết quả là các Dictionary

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetListReports", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@user", user);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            // Duyệt qua từng dòng (record)
            while (await reader.ReadAsync())
            {
                var report = new Dictionary<string, object>();

                // Duyệt qua tất cả các cột trong dòng và thêm vào Dictionary
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    // Lấy tên cột và giá trị của cột
                    var columnName = reader.GetName(i);  // Tên cột
                    var columnValue = reader.GetValue(i); // Giá trị cột

                    // Thêm vào Dictionary với tên cột làm key và giá trị cột làm value
                    report[columnName] = columnValue;
                }

                reports.Add(report);  // Thêm Dictionary vào danh sách
            }

            return reports;  // Trả về danh sách chứa các báo cáo
        }

        public async Task<List<Dictionary<string, object>>> ProcessDynamicDataAsync(string sysID,string processedFilters, int currentPage, int pageSize)
        {
            var reports = new List<Dictionary<string, object>>();

           
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(processedFilters, conn); // Sử dụng tên stored procedure động
            cmd.CommandType = CommandType.Text;

           
            // Thực thi câu lệnh SQL
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var report = new Dictionary<string, object>();

                // Lặp qua tất cả các cột trong dòng và thêm vào Dictionary
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var columnValue = reader.GetValue(i);
                    report[columnName] = columnValue;
                }

                reports.Add(report);
            }

            return reports;
        }

        public async Task<DataSet> ProcessDataSetReport(string sysID, string processedFilters, int currentPage, int pageSize)
        {
            // Tạo DataSet để chứa dữ liệu
            DataSet dataSet = new DataSet();


            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(processedFilters, conn); // Sử dụng tên stored procedure động
            cmd.CommandType = CommandType.Text;


            // Thực thi câu lệnh SQL
            await conn.OpenAsync();
            // Tạo SqlDataAdapter để điền dữ liệu vào DataSet
            using (var adapter = new SqlDataAdapter(cmd))
            {
                // Mở kết nối
                await conn.OpenAsync();

                // Điền dữ liệu vào DataSet
                adapter.Fill(dataSet);
            }

            return dataSet;
        }

        public async Task<(List<Report> Reports, int TotalCount)> _GetReportsAsync(int page, int pageSize, string search)
        {
            var reports = new List<Report>();
            int totalCount = 0;

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetReportsPaged", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@Search", search ?? string.Empty);

            var outParam = new SqlParameter("@TotalCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outParam);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reports.Add(new Report
                {
                    Id = reader.GetString(1),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    CreatedDate = reader.GetDateTime(3)
                });
            }

            // Lấy giá trị của tham số output
            if (outParam.Value != DBNull.Value)
            {
                totalCount = (int)outParam.Value;
            }
            return (reports, totalCount);
        }

        public async Task<Report> GetReportByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("GetReportById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Report
                {
                    Id = reader.GetString(1),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    CreatedDate = reader.GetDateTime(3)
                };
            }

            return null;
        }

        public async Task<List<ListBoxItem>> GetListBoxDataAsync(string sysID, string filterID)
        {
            // Lấy cấu hình vlookup theo code (valid)
            var _vlookupService = new VLookupService();
            VLookup vlookup = await _vlookupService.GetVLookupByCodeAsync(filterID); // Dùng filterID làm code

            if (vlookup == null)
                return null;

            // Xây dựng câu lệnh SQL dựa trên cấu hình vlookup
            // Ví dụ: SELECT Id, Name FROM Reports WHERE IsActive=1
            string fields = string.Join(", ", vlookup.Fields);
            string sqlQuery = $"SELECT {fields} FROM {vlookup.Table}";

            if (!string.IsNullOrWhiteSpace(vlookup.Where))
            {
                sqlQuery += $" WHERE {vlookup.Where}";
            }
            var items = new List<ListBoxItem>();

            using var conn = new SqlConnection(_connectionString);
            using (conn)
            {
                using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Giả sử cột đầu tiên là Value và cột thứ hai là Text
                            items.Add(new ListBoxItem
                            {
                                Value = reader[0].ToString(),
                                Text = reader[1].ToString()
                            });
                        }
                    }
                }
            }

            return items;
        }

        public async Task<List<DataSet>> getDataSetReport(string processedFilters)
        {
            var ds = new List<DataSet>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(processedFilters, conn); // Sử dụng tên stored procedure động
            cmd.CommandType = CommandType.Text;


            // Thực thi câu lệnh SQL
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (!reader.IsClosed) // Duyệt qua từng ResultSet
            {
                var dataTable = new DataTable();
                dataTable.Load(reader);

                var dataSet = new DataSet();
                dataSet.Tables.Add(dataTable);

                ds.Add(dataSet);
            }

            return ds;
        }
        public async Task<DataSet> getDataSetSystemReport(string processedFilters)
        {
            var ds = new DataSet();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(processedFilters, conn); // Sử dụng tên stored procedure động
            cmd.CommandType = CommandType.Text;


            // Thực thi câu lệnh SQL
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            conn.Open();
            adapter.Fill(ds);
            conn.Close();
            return ds;
        }

        public async Task<ReportResponse> SetReportData(
        string sysID,
        string storeProcName,
        int userId,
        int currentPage,
        int pageSize,
        Dictionary<string, object> processedFilters)
        {
            var reports = new List<Dictionary<string, object>>();
            var masterData = new List<Dictionary<string, object>>();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd = new SqlCommand(storeProcName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Thêm các tham số vào SqlCommand sử dụng SQL Parameters
                foreach (var param in processedFilters)
                {
                    // Đảm bảo rằng tham số không bị null
                    cmd.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }

                // Thêm tham số phân trang
                //cmd.Parameters.AddWithValue("@CurrentPage", currentPage);
                //cmd.Parameters.AddWithValue("@PageSize", pageSize);

                // Thêm tham số OUTPUT nếu cần (không cần nếu bạn đang sử dụng result sets cho MasterData)
                // Nếu stored procedure trả về MasterData dưới dạng result set, bạn không cần tham số OUTPUT

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // Đọc dữ liệu Reports từ result set đầu tiên
                while (await reader.ReadAsync())
                {
                    var report = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var columnValue = reader.GetValue(i);
                        report[columnName] = columnValue;
                    }

                    reports.Add(report);
                }

                // Chuyển sang result set thứ hai (MasterData)
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var masterDataEntry = new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var columnValue = reader.GetValue(i);
                            masterDataEntry[columnName] = columnValue;
                        }

                        masterData.Add(masterDataEntry);
                    }
                }

                // Nếu không có result set thứ hai, tạo MasterData từ output parameters hoặc tính toán
                if (masterData.Count == 0)
                {
                    int totalItems = 0;
                    if (processedFilters.ContainsKey("TotalItems") && processedFilters["TotalItems"] is int ti)
                    {
                        totalItems = ti;
                    }
                    else
                    {
                        totalItems = reports.Count; // Ví dụ đơn giản
                    }

                    int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                    var masterDataEntry = new Dictionary<string, object>
                {
                    { "TotalItems", totalItems },
                    { "TotalPages", totalPages },
                    { "CurrentPage", currentPage },
                    { "PageSize", pageSize }
                };
                    masterData.Add(masterDataEntry);
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về phản hồi lỗi
                return new ReportResponse
                {
                    Status = "0",
                    Message = $"Error processing dynamic data: {ex.Message}",
                    Reports = null,
                    MasterData = null
                };
            }

            return new ReportResponse
            {
                Status = "1",
                Message = "Successfully",
                Reports = reports,
                MasterData = masterData
            };
        }

        public async Task<string> GetConnectionStringByReportNameAsync(string sysID)
        {
            // Chuỗi kết nối sẽ được trả về
            string result = string.Empty;

            // SQL query
            string query = @"
            SELECT ISNULL(b.ConnectionString, '') AS ConnectionString
            FROM Reports a
            LEFT JOIN ReportConnections b ON a.ConnectionId = b.ConnectionId
            WHERE a.ReportName = @sysID";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Thêm tham số @sysID
                        command.Parameters.AddWithValue("@sysID", sysID);

                        // Thực hiện truy vấn
                        var connectionStringValue = await command.ExecuteScalarAsync();

                        if (connectionStringValue != null)
                        {
                            result = connectionStringValue.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi hoặc xử lý lỗi
                throw new Exception($"Error fetching ConnectionString for report '{sysID}': {ex.Message}", ex);
            }

            return result;
        }

    }


}
