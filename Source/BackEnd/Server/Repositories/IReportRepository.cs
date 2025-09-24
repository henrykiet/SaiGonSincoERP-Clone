using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using reportSystem01.Server.Models;
using  reportSystem01.Shared;

namespace ReportServer.Repositories
{
    public interface IReportRepository
    {
        Task<List<Dictionary<string, object>>> GetReportsAsync(int user);
        Task<List<Dictionary<string, object>>> ProcessDynamicDataAsync(string sysID, string processedFilters, int currentPage, int pageSize);
        Task<(List<Report> Reports, int TotalCount)> _GetReportsAsync(int page, int pageSize, string search);
        
        Task<Report> GetReportByIdAsync(int id);
        Task<List<ListBoxItem>> GetListBoxDataAsync(string sysID, string filterID);
        Task<List<DataSet>> getDataSetReport(string filterID);
        Task<DataSet> getDataSetSystemReport(string filterID);
        Task<ReportResponse> SetReportData(
       string sysID,
       string storeProcName,
       int userId,
       int currentPage,
       int pageSize,
       Dictionary<string, object> processedFilters);
    }
}
