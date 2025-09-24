using reportSystem01.Shared;
using Sinco.Server.Models;

namespace Sinco.Server.Repositories.Custom
{
    public interface IUnitRepository
    {
        Task<ServiceResponse<List<UnitDto>>> GetAllUnitsAsync();
    }
}
