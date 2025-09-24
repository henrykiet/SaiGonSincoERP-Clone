using Microsoft.Extensions.Configuration;
using Sinco.Server.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Sinco.Server.Repositories.AttachedFile
{
    public class AttachedFileRepository : IAttachedFileRepository
    {
        private readonly string _connectionString;

        public AttachedFileRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> UploadFileAsync(FileAttachment file)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"INSERT INTO Files (Controller, sysKey, FileName, FileContent, ContentType) 
                      VALUES (@Controller, @SysKey, @FileName, @FileContent, @ContentType)", connection))
                {
                    command.Parameters.AddWithValue("@Controller", file.Controller);
                    command.Parameters.AddWithValue("@SysKey", file.SysKey);
                    command.Parameters.AddWithValue("@FileName", file.FileName);
                    command.Parameters.AddWithValue("@FileContent", file.FileContent);
                    command.Parameters.AddWithValue("@ContentType", file.ContentType);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<IEnumerable<FileAttachment>> GetFilesAsync(string controller, string sysKey)
        {
            var files = new List<FileAttachment>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT FileName, ContentType, FileContent FROM Files 
                      WHERE Controller = @Controller AND sysKey = @SysKey", connection))
                {
                    command.Parameters.AddWithValue("@Controller", controller);
                    command.Parameters.AddWithValue("@SysKey", sysKey);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            files.Add(new FileAttachment
                            {
                                Controller = controller,
                                SysKey = sysKey,
                                FileName = reader.GetString(0),
                                FileContent = (byte[])reader["FileContent"],
                                ContentType = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return files;
        }

        public async Task<FileAttachment> GetFileAsync(string controller, string sysKey, string fileName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT FileContent, ContentType FROM Files 
                      WHERE Controller = @Controller AND sysKey = @SysKey AND FileName = @FileName", connection))
                {
                    //command.Parameters.AddWithValue("@Controller", controller);
                    //command.Parameters.AddWithValue("@SysKey", sysKey);
                    //command.Parameters.AddWithValue("@FileName", fileName);
                    command.Parameters.AddWithValue("@Controller", "customer");
                    command.Parameters.AddWithValue("@SysKey", "KH001");
                    command.Parameters.AddWithValue("@FileName", fileName);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new FileAttachment
                            {
                                Controller = controller,
                                SysKey = sysKey,
                                FileName = fileName,
                                FileContent = (byte[])reader["FileContent"],
                                ContentType = reader["ContentType"].ToString()
                            };
                        }
                    }
                }
            }

            return null;
        }

        public async Task<bool> DeleteFileAsync(string controller, string sysKey, string fileName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"DELETE FROM Files 
                      WHERE Controller = @Controller AND sysKey = @SysKey AND FileName = @FileName", connection))
                {
                    command.Parameters.AddWithValue("@Controller", controller);
                    command.Parameters.AddWithValue("@SysKey", sysKey);
                    command.Parameters.AddWithValue("@FileName", fileName);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }
    }
}