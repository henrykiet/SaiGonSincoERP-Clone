using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Sinco.Server.Models;
using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Custom
{

    public class UnitRepository : IUnitRepository
    {
        private readonly string _connectionString;

        public UnitRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<ServiceResponse<List<UnitDto>>> GetAllUnitsAsync()
        {
            try
            {
                var units = new List<UnitDto>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("SELECT UnitCode, UnitName, UnitName2 FROM Unit ORDER BY UnitName", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                units.Add(new UnitDto
                                {
                                    UnitCode = reader["UnitCode"].ToString() ?? string.Empty,
                                    UnitName = reader["UnitName"].ToString() ?? string.Empty,
                                    UnitName2 = reader["UnitName2"].ToString() ?? string.Empty
                                });
                            }
                        }
                    }
                }

                return ServiceResponse<List<UnitDto>>.CreateSuccess(units, "Lấy danh sách đơn vị thành công");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error getting units: {ex.Message}");
                return ServiceResponse<List<UnitDto>>.CreateError($"Lỗi khi lấy danh sách đơn vị: {ex.Message}", 500);
            }
        }
    }
}