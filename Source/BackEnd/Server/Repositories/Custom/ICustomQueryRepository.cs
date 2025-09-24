using Sinco.Server.Models;

namespace Sinco.Server.Repositories.Custom
{
    public interface ICustomQueryRepository
    {
        Task<List<Dictionary<string, object>>> ExecuteCustomQueryAsync(CustomQueryRequest request);
    }
}
