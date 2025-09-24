//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;
using System.Data;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using reportSystem01.Server.Models;
using ReportServer.Repositories;
using reportSystem01.Shared;
using static reportSystem01.Shared.ReportFilter;


namespace ReportServer.Services
{
    
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;

        public ReportService(IReportRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Dictionary<string, object>>> GetReportsAsync(int user)
        {
            return _repo.GetReportsAsync(user);
        }

        public Task<(List<Report> Reports, int TotalCount)> _GetReportsAsync(int page, int pageSize, string search)
        {
            return _repo._GetReportsAsync(page, pageSize, search);
        }
        public Task<List<Dictionary<string, object>>> ProcessDynamicDataAsync(string sysID, string processedFilters, int currentPage, int pageSize)
        {
            return _repo.ProcessDynamicDataAsync(sysID, processedFilters, currentPage, pageSize);
        }

        public Task<Report> GetReportByIdAsync(int id)
        {
            return _repo.GetReportByIdAsync(id);
        }

        public Task<List<ListBoxItem>> GetListBoxDataAsync(string sysID, string filterID)
        {
            return _repo.GetListBoxDataAsync(sysID, filterID);
        }

        public Stream GetReportPdf(int id)
        {
            // Load Crystal Report
            //ReportDocument rd = new ReportDocument();
            //rd.Load("Reports/MyReport.rpt");
            //// Thiết lập tham số, logon info nếu cần
            //// vd: rd.SetDatabaseLogon("user","pass","server","db");
            //rd.SetParameterValue("ReportId", id); // Giả sử trong RPT có param này

            //Stream pdfStream = rd.ExportToStream(ExportFormatType.PortableDocFormat);
            //pdfStream.Seek(0, SeekOrigin.Begin);
            //return pdfStream;
            return null;
        }

        public Task<List<DataSet>>  getDataSetReport(string filterID)
        {
            return _repo.getDataSetReport(filterID);
        }
        public Task<DataSet> getDataSetSystemReport(string filterID)
        {
            return _repo.getDataSetSystemReport(filterID);
        }

        public Task<ReportResponse> SetReportData(
       string sysID,
       string storeProcName,
       int userId,
       int currentPage,
       int pageSize,
       Dictionary<string, object> processedFilters)
        {
            return _repo.SetReportData(sysID, storeProcName, userId, currentPage, pageSize, processedFilters);
        }
    }
}
