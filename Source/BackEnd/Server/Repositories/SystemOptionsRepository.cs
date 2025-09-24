using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using reportSystem01.Shared;

public class SystemOptionsRepository
{
    private readonly string _connectionString;

    public SystemOptionsRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<SystemOption>> GetAllAsync()
    {
        var options = new List<SystemOption>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM SystemOptions", conn))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        options.Add(new SystemOption
                        {
                            Id = reader.GetInt32(0),
                            ConfigKey = reader.GetString(1),
                            ConfigValue = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            userGroup = reader.GetString(6)
                        });
                    }
                }
            }
        }

        return options;
    }

    public async Task<List<SystemOption>> GetAllOptionsAsyncGroup(string group)
    {
        var options = new List<SystemOption>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT * FROM SystemOptions WHERE ([userGroup] = @group)", conn);
        cmd.Parameters.AddWithValue("@group", group);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            options.Add(new SystemOption
            {
                Id = reader.GetInt32(0),
                ConfigKey = reader.GetString(1),
                ConfigValue = reader.IsDBNull(2) ? null : reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                CreatedAt = reader.GetDateTime(4),
                UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                userGroup = reader.GetString(6)
            });
        }
        return options;
    }

    public async Task<List<string>> GetAllGroupsAsync()
    {
        var groups = new List<string>();
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT DISTINCT [userGroup] FROM SystemOptions", conn);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            groups.Add(reader.GetString(0));
        }
        return groups;
    }

    public async Task CreateOptionAsync(SystemOption option)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("INSERT INTO SystemOptions (userGroup, ConfigKey, ConfigValue, Description) VALUES (@group, @key, @value, @desc)", conn);
        cmd.Parameters.AddWithValue("@group", option.userGroup);
        cmd.Parameters.AddWithValue("@key", option.ConfigKey);
        cmd.Parameters.AddWithValue("@value", option.ConfigValue);
        cmd.Parameters.AddWithValue("@desc", option.Description);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    public async Task<SystemOption> GetByIdAsync(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM SystemOptions WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new SystemOption
                        {
                            Id = reader.GetInt32(0),
                            ConfigKey = reader.GetString(1),
                            ConfigValue = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            userGroup = reader.GetString(6)
                        };
                    }
                }
            }
        }
        return null;
    }

    public async Task<int> InsertAsync(SystemOption option)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO SystemOptions (ConfigKey, ConfigValue, Description, CreatedAt)
                VALUES (@ConfigKey, @ConfigValue, @Description, GETDATE())", conn))
            {
                cmd.Parameters.AddWithValue("@ConfigKey", option.ConfigKey);
                cmd.Parameters.AddWithValue("@ConfigValue", option.ConfigValue ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", option.Description ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<int> UpdateAsync(SystemOption option)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE SystemOptions 
                SET ConfigValue = @ConfigValue, Description = @Description, UpdatedAt = GETDATE()
                WHERE ConfigKey = @ConfigKey", conn))
            {
                cmd.Parameters.AddWithValue("@ConfigKey", option.ConfigKey);
                cmd.Parameters.AddWithValue("@ConfigValue", option.ConfigValue ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", option.Description ?? (object)DBNull.Value);

                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<int> DeleteAsync(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand("DELETE FROM SystemOptions WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
