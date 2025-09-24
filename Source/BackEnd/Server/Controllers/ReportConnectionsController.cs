using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using reportSystem01.Shared;
using reportSystem01.Server.Services;
using log4net;
namespace reportSystem01.Server.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]

    public class ReportConnectionsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportConnectionsController> _logger;

        public ReportConnectionsController(IConfiguration configuration, ILogger<ReportConnectionsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // GET: api/ReportConnections
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportConnection>>> GetReportConnections()
        {
            var connections = new List<ReportConnection>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = "SELECT ConnectionId, ConnectionName, DbType, ConnectionString, IsActive, CreatedAt FROM ReportConnections";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                connections.Add(new ReportConnection
                                {
                                    ConnectionId = reader.GetInt32(0),
                                    ConnectionName = reader.GetString(1),
                                    DbType = reader.GetString(2),
                                    ConnectionString = reader.GetString(3),
                                    IsActive = reader.GetBoolean(4),
                                    CreatedAt = reader.GetDateTime(5)
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("Đã lấy danh sách kết nối thành công.");
                return Ok(connections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách kết nối.");
                return StatusCode(500, "Đã xảy ra lỗi khi lấy danh sách kết nối.");
            }
        }

        // GET: api/ReportConnections/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportConnection>> GetReportConnection(int id)
        {
            ReportConnection connection = null;

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = "SELECT ConnectionId, ConnectionName, DbType, ConnectionString, IsActive, CreatedAt FROM ReportConnections WHERE ConnectionId = @ConnectionId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConnectionId", id);
                        cmd.CommandType = CommandType.Text;
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                connection = new ReportConnection
                                {
                                    ConnectionId = reader.GetInt32(0),
                                    ConnectionName = reader.GetString(1),
                                    DbType = reader.GetString(2),
                                    ConnectionString = reader.GetString(3),
                                    IsActive = reader.GetBoolean(4),
                                    CreatedAt = reader.GetDateTime(5)
                                };
                            }
                        }
                    }
                }

                if (connection == null)
                {
                    _logger.LogWarning($"Không tìm thấy kết nối với ID: {id}");
                    return NotFound();
                }

                _logger.LogInformation($"Đã lấy kết nối với ID: {id}");
                return Ok(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy kết nối với ID: {id}");
                return StatusCode(500, "Đã xảy ra lỗi khi lấy kết nối.");
            }
        }

        // POST: api/ReportConnections
        [HttpPost]
        public async Task<ActionResult<ReportConnection>> PostReportConnection(ReportConnection reportConnection)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = @"
                INSERT INTO ReportConnections (ConnectionName, DbType, ConnectionString, IsActive, CreatedAt)
                VALUES (@ConnectionName, @DbType, @ConnectionString, @IsActive, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConnectionName", reportConnection.ConnectionName);
                        cmd.Parameters.AddWithValue("@DbType", reportConnection.DbType);
                        cmd.Parameters.AddWithValue("@ConnectionString", reportConnection.ConnectionString);
                        cmd.Parameters.AddWithValue("@IsActive", reportConnection.IsActive);
                        cmd.Parameters.AddWithValue("@CreatedAt", reportConnection.CreatedAt);

                        cmd.CommandType = CommandType.Text;
                        await conn.OpenAsync();
                        var result = await cmd.ExecuteScalarAsync();
                        reportConnection.ConnectionId = Convert.ToInt32(result);
                    }
                }

                _logger.LogInformation($"Đã thêm kết nối mới với ID: {reportConnection.ConnectionId}");
                return CreatedAtAction(nameof(GetReportConnection), new { id = reportConnection.ConnectionId }, reportConnection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm kết nối mới.");
                return StatusCode(500, "Đã xảy ra lỗi khi thêm kết nối mới.");
            }
        }

        // PUT: api/ReportConnections/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReportConnection(int id, ReportConnection reportConnection)
        {
            if (id != reportConnection.ConnectionId)
            {
                return BadRequest("ID không khớp.");
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = @"
                UPDATE ReportConnections 
                SET ConnectionName = @ConnectionName, 
                    DbType = @DbType, 
                    ConnectionString = @ConnectionString, 
                    IsActive = @IsActive
                WHERE ConnectionId = @ConnectionId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConnectionName", reportConnection.ConnectionName);
                        cmd.Parameters.AddWithValue("@DbType", reportConnection.DbType);
                        cmd.Parameters.AddWithValue("@ConnectionString", reportConnection.ConnectionString);
                        cmd.Parameters.AddWithValue("@IsActive", reportConnection.IsActive);
                        cmd.Parameters.AddWithValue("@ConnectionId", id);

                        cmd.CommandType = CommandType.Text;
                        await conn.OpenAsync();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            _logger.LogWarning($"Không tìm thấy kết nối với ID: {id} để cập nhật.");
                            return NotFound();
                        }
                    }
                }

                _logger.LogInformation($"Đã cập nhật kết nối với ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật kết nối với ID: {id}");
                return StatusCode(500, "Đã xảy ra lỗi khi cập nhật kết nối.");
            }
        }
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] ReportConnection connection)
        {
            try
            {
                if (connection == null || string.IsNullOrEmpty(connection.ConnectionString))
                {
                    return BadRequest(new { message = "Connection string is required" });
                }

                bool isSuccess = false;
                string dbType = connection.DbType?.ToLower();

                switch (dbType)
                {
                    case "sql server":
                        try
                        {
                            using (var conn = new SqlConnection(connection.ConnectionString))
                            {
                                await conn.OpenAsync();
                                isSuccess = true;
                            }
                        }
                        catch
                        {
                            isSuccess = false;
                        }
                        
                        break;
                    case "mysql":
                        //try
                        //{
                        //    using (var conn = new MySqlConnection(connectionString))
                        //    {
                        //        await conn.OpenAsync();
                        //        isSuccess = true;
                        //    }
                        //}
                        //catch
                        //{
                        //    isSuccess = false;
                        //}
                        isSuccess = false;
                        break;
                    case "oracle":
                        //try
                        //{
                        //    using (var conn = new OracleConnection(connectionString))
                        //    {
                        //        await conn.OpenAsync();
                        //        isSuccess = true ;
                        //    }
                        //}
                        //catch
                        //{
                        //    isSuccess = false;
                        //}
                        isSuccess = false;
                        break;
                    default:
                        return BadRequest(new { message = "Unsupported database type." });
                }

                return isSuccess ? Ok(new { message = "Connection successful!" }) :
                                   StatusCode(500, new { message = "Connection failed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to database.");
                return StatusCode(500, new { message = "Connection failed: " + ex.Message });
            }
        }


        // DELETE: api/ReportConnections/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReportConnection(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = "DELETE FROM ReportConnections WHERE ConnectionId = @ConnectionId";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConnectionId", id);
                        cmd.CommandType = CommandType.Text;
                        await conn.OpenAsync();
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            _logger.LogWarning($"Không tìm thấy kết nối với ID: {id} để xóa.");
                            return NotFound();
                        }
                    }
                }

                _logger.LogInformation($"Đã xóa kết nối với ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa kết nối với ID: {id}");
                return StatusCode(500, "Đã xảy ra lỗi khi xóa kết nối.");
            }
        }
    }
}
