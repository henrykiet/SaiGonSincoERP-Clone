using Microsoft.AspNetCore.Mvc;
using reportSystem01.Server.Models;
using ReportServer.Repositories;
using ReportServer.Services;
using static reportSystem01.Shared.ReportFilter;
using static reportSystem01.Shared.ListBoxItem;
using reportSystem01.Shared;
using System.Data;
namespace ReportServer.Services
{
    public interface IReportService
    {
        Task<List<Dictionary<string, object>>> GetReportsAsync(int user);
        Task<(List<Report> Reports, int TotalCount)> _GetReportsAsync(int page, int pageSize, string search);
        Task<Report> GetReportByIdAsync(int id);
        Task<List<Dictionary<string, object>>> ProcessDynamicDataAsync(string sysID, string processedFilters, int currentPage, int pageSize);

        Stream GetReportPdf(int id);
        Task<List<ListBoxItem>> GetListBoxDataAsync(string sysID, string filterID);
        Task<List<DataSet>> getDataSetReport(string processedFilters);
        Task<DataSet> getDataSetSystemReport(string processedFilters);
        Task<ReportResponse> SetReportData(
       string sysID,
       string storeProcName,
       int userId,
       int currentPage,
       int pageSize,
       Dictionary<string, object> processedFilters);
    }
}
