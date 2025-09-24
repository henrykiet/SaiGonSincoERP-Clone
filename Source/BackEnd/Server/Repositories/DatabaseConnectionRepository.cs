using Microsoft.Data.SqlClient;
using reportSystem01.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class DatabaseConnectionRepository : IDatabaseConnectionRepository
{
    private readonly string _connectionString;

    public DatabaseConnectionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<DatabaseConnection>> GetAllConnectionsAsync()
    {
        var connections = new List<DatabaseConnection>();

        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand("SELECT * FROM DatabaseConnections", conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    connections.Add(new DatabaseConnection
                    {
                        Id = reader.GetInt32(0),
                        ConnectionName = reader.GetString(1),
                        DbType = reader.GetString(2),
                        Server = reader.GetString(3),
                        Port = reader.GetInt32(4),
                        DatabaseName = reader.GetString(5),
                        Username = reader.GetString(6),
                        Password = reader.GetString(7),
                        IsActive = reader.GetBoolean(8),
                        SID = reader.GetString(9)
                    });
                }
            }
        }

        return connections;
    }

    public async Task<DatabaseConnection> GetConnectionByIdAsync(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand("SELECT * FROM DatabaseConnections WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new DatabaseConnection
                        {
                            Id = reader.GetInt32(0),
                            ConnectionName = reader.GetString(1),
                            DbType = reader.GetString(2),
                            Server = reader.GetString(3),
                            Port = reader.GetInt32(4),
                            DatabaseName = reader.GetString(5),
                            Username = reader.GetString(6),
                            Password = reader.GetString(7),
                            IsActive = reader.GetBoolean(8),
                            SID = reader.GetString(9)
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task<bool> CreateConnectionAsync(DatabaseConnection connection)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand(@"
                INSERT INTO DatabaseConnections 
                (ConnectionName, DbType, Server, Port, DatabaseName, Username, Password, IsActive, SID) 
                VALUES (@ConnectionName, @DbType, @Server, @Port, @DatabaseName, @Username, @Password, @IsActive, @SID)", conn))
            {
                cmd.Parameters.AddWithValue("@ConnectionName", connection.ConnectionName);
                cmd.Parameters.AddWithValue("@DbType", connection.DbType);
                cmd.Parameters.AddWithValue("@Server", connection.Server);
                cmd.Parameters.AddWithValue("@Port", connection.Port);
                cmd.Parameters.AddWithValue("@DatabaseName", connection.DatabaseName);
                cmd.Parameters.AddWithValue("@Username", connection.Username);
                cmd.Parameters.AddWithValue("@Password", connection.Password);
                cmd.Parameters.AddWithValue("@IsActive", connection.IsActive);
                cmd.Parameters.AddWithValue("@SID", connection.SID);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

    public async Task<bool> UpdateConnectionAsync(DatabaseConnection connection)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand(@"
                UPDATE DatabaseConnections 
                SET ConnectionName=@ConnectionName, DbType=@DbType, Server=@Server, Port=@Port, 
                    DatabaseName=@DatabaseName, Username=@Username, Password=@Password, IsActive=@IsActive, SID = @SID 
                WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", connection.Id);
                cmd.Parameters.AddWithValue("@ConnectionName", connection.ConnectionName);
                cmd.Parameters.AddWithValue("@DbType", connection.DbType);
                cmd.Parameters.AddWithValue("@Server", connection.Server);
                cmd.Parameters.AddWithValue("@Port", connection.Port);
                cmd.Parameters.AddWithValue("@DatabaseName", connection.DatabaseName);
                cmd.Parameters.AddWithValue("@Username", connection.Username);
                cmd.Parameters.AddWithValue("@Password", connection.Password);
                cmd.Parameters.AddWithValue("@IsActive", connection.IsActive);
                cmd.Parameters.AddWithValue("@SID", connection.SID);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

    public async Task<bool> DeleteConnectionAsync(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (var cmd = new SqlCommand("DELETE FROM DatabaseConnections WHERE Id=@Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }

    public async Task<bool> TestConnectionAsync(DatabaseConnection connection)
    {
        try
        {
            using var conn = connection.DbType switch
            {
                "SQL Server" => new SqlConnection(connection.ConnectionString),
               // "MySQL" => new MySqlConnection(connection.ConnectionString),
                //"Oracle" => new OracleConnection(connection.ConnectionString),
                _ => throw new NotSupportedException("Database type not supported")
            };

            await conn.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
