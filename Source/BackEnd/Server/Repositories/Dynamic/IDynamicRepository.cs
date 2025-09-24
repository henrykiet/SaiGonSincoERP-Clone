using Sinco.Server.Models;
using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Dynamic
{
    public interface IDynamicRepository
    {
        Task<DynamicQueryResponse<Dictionary<string, object>>> GetDynamicListAsync(DynamicQueryRequest request);
        Task<DynamicQueryResponse<Dictionary<string, object>>> GetDynamicFilterListAsync(DynamicFilterRequest request);
        Task<Dictionary<string, object>> GetDynamicByIdAsync(string formId, string[] primaryKeys, string[] values);
        Task<DynamicSaveResponse> SaveDynamicAsync(DynamicSaveRequest request);
        Task<DynamicDeleteResponse> DeleteDynamicAsync(DynamicDeleteRequest request);

        Task<ServiceResponse<string>> GetNextFieldNumberAsync(string controller, string field, string formId = null);

    }
} 