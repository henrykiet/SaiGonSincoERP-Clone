using System.Collections.Generic;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public interface IDatabaseConnectionRepository
    {
        Task<List<DatabaseConnection>> GetAllConnectionsAsync();
        Task<DatabaseConnection> GetConnectionByIdAsync(int id);
        Task<bool> UpdateConnectionAsync(DatabaseConnection connection);
        Task<bool> DeleteConnectionAsync(int id);
        Task<bool> CreateConnectionAsync(DatabaseConnection connection);
        Task<bool> TestConnectionAsync(DatabaseConnection connection);
    }
}
