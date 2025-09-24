using Sinco.Server.Models;

namespace Sinco.Server.Repositories.GetFormData
{
    public interface IFormConfigRepository
    {
        Task<string> GetFormConfigAsync(string fileName);
        Task<object> SyncDataFromFormAdvancedAsync(SyncRequest request, dynamic formConfig);
        Task<Dictionary<string, object>> GetFormDataAsync(
            string tableName,
            string[] primaryKeys,
            string[] values,
            string unit,
            string userId,
            string type = "list",
            string vcDate = "");
        Task<List<Dictionary<string, object>>> GetDetailDataAsync(
            string tableName,
            string[] foreignKeys,
            string[] values,
            string unit,
            string userId,
            string type = "list",
            string vcDate = "");
    }
}