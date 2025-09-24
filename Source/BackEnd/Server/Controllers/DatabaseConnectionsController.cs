using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using reportSystem01.Shared;
using Microsoft.AspNetCore.Authorization;

[Route("api/database-connections")]
[ApiController]
[Authorize]
public class DatabaseConnectionsController : ControllerBase
{
    private readonly IDatabaseConnectionRepository _repository;
    private readonly ILogger<DatabaseConnectionsController> _logger;

    public DatabaseConnectionsController(IDatabaseConnectionRepository repository, ILogger<DatabaseConnectionsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // Lấy danh sách tất cả kết nối
    [HttpGet]
    public async Task<IActionResult> GetAllConnections()
    {
        try
        {
            var connections = await _repository.GetAllConnectionsAsync();
            return Ok(connections);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving connections: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    // Lấy kết nối theo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConnectionById(int id)
    {
        try
        {
            var connection = await _repository.GetConnectionByIdAsync(id);
            if (connection == null)
            {
                return NotFound(new { message = "Connection not found." });
            }
            return Ok(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving connection ID {id}: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    // Thêm kết nối mới
    [HttpPost]
    public async Task<IActionResult> CreateConnection([FromBody] DatabaseConnection connection)
    {
        try
        {
            if (connection == null)
            {
                return BadRequest(new { message = "Invalid connection data." });
            }

             await _repository.CreateConnectionAsync(connection);

            return Ok(new { message = "Connection added successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating connection: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    // Cập nhật kết nối
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConnection(int id, [FromBody] DatabaseConnection connection)
    {
        try
        {
            if (connection == null || id != connection.Id)
            {
                return BadRequest(new { message = "Invalid connection data." });
            }

            var existingConnection = await _repository.GetConnectionByIdAsync(id);
            if (existingConnection == null)
            {
                return NotFound(new { message = "Connection not found." });
            }

            await _repository.UpdateConnectionAsync(connection);
            return Ok(new { message = "Connection updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating connection ID {id}: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    // Xóa kết nối
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConnection(int id)
    {
        try
        {
            var existingConnection = await _repository.GetConnectionByIdAsync(id);
            if (existingConnection == null)
            {
                return NotFound(new { message = "Connection not found." });
            }

            await _repository.DeleteConnectionAsync(id);
            return Ok(new { message = "Connection deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting connection ID {id}: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    // Kiểm tra kết nối cơ sở dữ liệu
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] DatabaseConnection connection)
    {
        try
        {
            bool isSuccess = await _repository.TestConnectionAsync(connection);
            return isSuccess ? Ok(new { message = "Connection successful!" }) :
                                  StatusCode(500, new { message = "Connection failed!" });
            //return Ok(new { success = isSuccess });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error testing connection: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }
}
