using Sinco.Server.Controllers;

namespace Sinco.Server.Repositories.Lookup
{
    public interface ILookupRepository
    {
        Task<object> LookupAsync(string controller, List<LookupController.FilterItem> filters, string language = "", string unit = "", string userId = "");
    }
}
