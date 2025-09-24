using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using reportSystem01.Server.Models;
using ReportServer.Repositories;
using ReportServer.Services;

namespace reportSystem01.Shared;

using System.Data;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Reporting.NETCore;
using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Utils;
using FastReport.Data;
using Azure.Core;
using log4net;
using Microsoft.AspNetCore.Authorization;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;
using ContentType = MimeKit.ContentType;
using Microsoft.Extensions.Options;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    private readonly EmailSettings _emailSettings;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ReportsController));

    public ReportsController(IReportService service, IOptions<EmailSettings> emailSettings)
    {
        _service = service;
        _emailSettings = emailSettings.Value;
    }



    [HttpGet("paged")]
    public async Task<IActionResult> GetReports([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null)
    {
        var (reports, totalCount) = await _service._GetReportsAsync(page, pageSize, search);
        return Ok(new { Data = reports, TotalCount = totalCount });
    }
    [HttpGet("list")]
    public async Task<IActionResult> GetListReports([FromQuery] int user = 1)
    {
        var reports = await _service.GetReportsAsync(user);
        if (reports == null)
            return NotFound();
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReportDetail(int id)
    {
        var report = await _service.GetReportByIdAsync(id);
        if (report == null)
            return NotFound();
        return Ok(report);
    }

    [HttpGet("{id}/pdf")]
    public IActionResult GetReportPdf(int id)
    {
        var pdfStream = _service.GetReportPdf(id);
        return File(pdfStream, "application/pdf", "report.pdf");
    }

    [HttpGet("test-log")]
    public IActionResult TestLog()
    {
        _logger.Debug("Đây là log DEBUG");
        _logger.Info("Đây là log INFO");
        _logger.Warn("Đây là log WARNING");
        _logger.Error("Đây là log ERROR");
        _logger.Fatal("Đây là log FATAL");

        return Ok("Ghi log thành công! Kiểm tra file Logs/app.log hoặc Console");
    }


    [HttpPost("process")]
    public async Task<ReportResponse> ProcessDynamicData([FromQuery] string sysID, [FromBody] Dictionary<string, object> requestData)
    {
        // Đọc các filter từ file XML
        //string pathxmlfilter = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Filter", sysID+".xml");
        ReportFilter reportFilter = new ReportFilter();
        var filtersFromXML = reportFilter.GetFiltersFromXml(sysID);
        var storeFromXML = reportFilter.GetStoreProcName(sysID);
        var response = new ReportResponse();
        // Truy xuất thông tin từ requestData
        if (requestData.ContainsKey("filters"))
        {
            var filtersObj = requestData["filters"];
            if (filtersObj is JsonElement jsonElement)
            {
                var processedFilters = new Dictionary<string, object>();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.ToString());
                if (filters != null)
                {
                    // Lặp qua các filter từ XML và kiểm tra trong requestData
                    foreach (var xmlFilter in filtersFromXML)
                    {
                        // Kiểm tra xem filter có trong requestData không
                        if (filters.ContainsKey(xmlFilter.ID))
                        {
                            // Lấy giá trị từ requestData
                            var filterValue = filters[xmlFilter.ID];

                            // Xử lý dữ liệu theo loại (type) từ file XML
                            if (xmlFilter.Type == "date" && DateTime.TryParse(filterValue.ToString(), out var dateValue))
                            {
                                string formattedFromDate = dateValue.ToString("yyyyMMdd");
                                processedFilters[xmlFilter.ID] = formattedFromDate;
                            }
                            else if (xmlFilter.Type == "int" && int.TryParse(filterValue.ToString(), out var intValue))
                            {
                                processedFilters[xmlFilter.ID] = intValue;
                            }
                            else if (xmlFilter.Type == "numeric" && decimal.TryParse(filterValue.ToString(), out var numValue))
                            {
                                processedFilters[xmlFilter.ID] = numValue;
                            }
                            else
                            {
                                processedFilters[xmlFilter.ID] = filterValue; // Nếu không có loại, dùng trực tiếp giá trị
                            }
                        }
                    }
                }
                // Lấy thông tin phân trang
                var currentPage = 1;
                var pageSize = 10;

                // Kiểm tra xem 'pagination' có tồn tại trong requestData không
                if (requestData.ContainsKey("pagination"))
                {
                    var paginationObj = requestData["pagination"];

                    // Kiểm tra xem paginationObj có phải là JsonElement không
                    if (paginationObj is JsonElement paginationElement)
                    {
                        // Deserialize JsonElement thành Dictionary<string, object>
                        var pagination = JsonSerializer.Deserialize<Dictionary<string, object>>(paginationElement.ToString());

                        if (pagination != null)
                        {
                            // Lấy giá trị currentPage nếu có, mặc định là 1
                            if (pagination.ContainsKey("currentPage"))
                            {
                                // Trích xuất giá trị currentPage từ JsonElement và chuyển sang int
                                currentPage = Convert.ToInt32(pagination["currentPage"].ToString());
                            }

                            // Lấy giá trị pageSize nếu có, mặc định là 10
                            if (pagination.ContainsKey("pageSize"))
                            {
                                // Trích xuất giá trị pageSize từ JsonElement và chuyển sang int
                                pageSize = Convert.ToInt32(pagination["pageSize"].ToString());
                            }
                        }
                    }
                }
                //thêm tham số user (để phân quyền trong db)
                var userOje = JsonSerializer.Deserialize<Dictionary<string, object>>(requestData["user"].ToString());
                var userID = userOje["userId"].ToString();
                processedFilters["userId"] = userID;
                // Thêm tham số phân trang
                processedFilters["currentPage"] = currentPage;
                processedFilters["pageSize"] = pageSize;

                // Lấy dữ liệu báo cáo từ repository

                var filterString = string.Join(", ", processedFilters.Values.Select(v => $"\'{v}\'"));
                filterString = "exec " + storeFromXML + " " + filterString;

                var reports = await _service.ProcessDynamicDataAsync(sysID, filterString, currentPage, pageSize);

                response.Status = "1";
                response.Message = "successfully";
                response.Reports = reports;
                //var response = new
                //{
                //    Status = "1",
                //    Message = "Data processed successfully",
                //    Reports = reports
                //};


            }
            else
            {
                response.Status = "0";
                response.Message = "'filters' should be a Dictionary<string, object>.";
                response.Reports = null;


            }
        }
        else
        {
            response.Status = "0";
            response.Message = "Missing 'filters' in the request data.";
            response.Reports = null;

        }

        return (response);

    }

    [HttpGet("getfilters")]
    public List<ReportFilter> GetFiltersFromXml([FromQuery] string sysID)
    {

        // Đọc XML từ thư mục server
        try
        {

            ReportFilter reportFilter = new ReportFilter();
            var filtersFromXML = reportFilter.GetFiltersFromXml(sysID);
            return filtersFromXML;
        }
        catch (Exception ex)
        {
            _logger.Error("Lỗi đọc XML từ thư mục server filter" + sysID + ".", ex);
        }
        return null;
    }
    [HttpGet("getBrowser")]
    public List<ReportGrid> GetBrowserFromXml([FromQuery] string sysID)
    {

        // Đọc XML từ thư mục server
        try
        {
            ReportGrid ReportGrid = new ReportGrid();
            var ReportGridFromXML = ReportGrid.GetBrowserFromXml(sysID);
            return ReportGridFromXML;
        }
        catch (Exception ex)
        {
            _logger.Error("Lỗi đọc XML từ thư mục server browser" + sysID + ".", ex);
        }
        return null;
        
    }
    // Endpoint để lấy dữ liệu cho ListBox Filter
    [HttpGet("listboxdata")]
    public async Task<IActionResult> GetListBoxData([FromQuery] string sysID, [FromQuery] string filterID)
    {
        var data = await _service.GetListBoxDataAsync(sysID, filterID);
        if (data == null || data.Count == 0)
        {
            _logger.Error("GetListBoxData "+ sysID+": Dữ liệu không tồn tại.");
            return NotFound("Dữ liệu không tồn tại.");
        }
        return Ok(data);
    }

    
    [HttpPost("exportpdf")]
    public async Task<IActionResult> ExportPdf([FromQuery] string sysID, [FromBody] Dictionary<string, object> requestData)
    {
        try
        {
            // Đường dẫn đến tệp báo cáo RDLC
               
            string baseDirectory = Directory.GetCurrentDirectory();
            string reportPath = Path.Combine(baseDirectory, "Controllers", "RDLC", sysID + ".rdlc");
            if (!System.IO.File.Exists(reportPath))
            {
                return NotFound(new { message = "Report file not found." });
            }

            // Tạo đối tượng LocalReport
            LocalReport report = new LocalReport();
            report.ReportPath = reportPath;

            // Thiết lập nguồn dữ liệu cho báo cáo
            ReportFilter reportFilter = new ReportFilter();
            var filtersFromXML = reportFilter.GetFiltersFromXml(sysID);
            var storeFromXML = reportFilter.GetStoreProcName(sysID);
            var response = new ReportResponse();
            // Truy xuất thông tin từ requestData
            var ds = new List<DataSet>();
            if (requestData.ContainsKey("filters"))
            {
                var filtersObj = requestData["filters"];
                if (filtersObj is JsonElement jsonElement)
                {
                    var processedFilters = new Dictionary<string, object>();
                    var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.ToString());
                    if (filters != null)
                    {
                        // Lặp qua các filter từ XML và kiểm tra trong requestData
                        foreach (var xmlFilter in filtersFromXML)
                        {
                            // Kiểm tra xem filter có trong requestData không
                            if (filters.ContainsKey(xmlFilter.ID))
                            {
                                // Lấy giá trị từ requestData
                                var filterValue = filters[xmlFilter.ID];

                                // Xử lý dữ liệu theo loại (type) từ file XML
                                if (xmlFilter.Type == "date" && DateTime.TryParse(filterValue.ToString(), out var dateValue))
                                {
                                    string formattedFromDate = dateValue.ToString("yyyyMMdd");
                                    processedFilters[xmlFilter.ID] = formattedFromDate;
                                }
                                else if (xmlFilter.Type == "int" && int.TryParse(filterValue.ToString(), out var intValue))
                                {
                                    processedFilters[xmlFilter.ID] = intValue;
                                }
                                else if (xmlFilter.Type == "numeric" && decimal.TryParse(filterValue.ToString(), out var numValue))
                                {
                                    processedFilters[xmlFilter.ID] = numValue;
                                }
                                else
                                {
                                    processedFilters[xmlFilter.ID] = filterValue; // Nếu không có loại, dùng trực tiếp giá trị
                                }
                            }
                        }
                    }
                    // Lấy thông tin phân trang
                    var currentPage = 1;
                    var pageSize = 10;

                    // Kiểm tra xem 'pagination' có tồn tại trong requestData không
                    if (requestData.ContainsKey("pagination"))
                    {
                        var paginationObj = requestData["pagination"];

                        // Kiểm tra xem paginationObj có phải là JsonElement không
                        if (paginationObj is JsonElement paginationElement)
                        {
                            // Deserialize JsonElement thành Dictionary<string, object>
                            var pagination = JsonSerializer.Deserialize<Dictionary<string, object>>(paginationElement.ToString());

                            if (pagination != null)
                            {
                                // Lấy giá trị currentPage nếu có, mặc định là 1
                                if (pagination.ContainsKey("currentPage"))
                                {
                                    // Trích xuất giá trị currentPage từ JsonElement và chuyển sang int
                                    currentPage = Convert.ToInt32(pagination["currentPage"].ToString());
                                }

                                // Lấy giá trị pageSize nếu có, mặc định là 10
                                if (pagination.ContainsKey("pageSize"))
                                {
                                    // Trích xuất giá trị pageSize từ JsonElement và chuyển sang int
                                    pageSize = Convert.ToInt32(pagination["pageSize"].ToString());
                                }
                            }
                        }
                    }
                    //thêm tham số user (để phân quyền trong db)
                    var userOje = JsonSerializer.Deserialize<Dictionary<string, object>>(requestData["user"].ToString());
                    var userID = userOje["userId"].ToString();
                    processedFilters["userId"] = userID;
                    // Thêm tham số phân trang
                    processedFilters["currentPage"] = 0;
                    processedFilters["pageSize"] = 0;

                    // Lấy dữ liệu báo cáo từ repository

                    var filterString = string.Join(", ", processedFilters.Values.Select(v => $"\'{v}\'"));
                    filterString = "exec " + storeFromXML + " " + filterString;

                    ds = await _service.getDataSetReport(filterString);

                   
                    //var response = new
                    //{
                    //    Status = "1",
                    //    Message = "Data processed successfully",
                    //    Reports = reports
                    //};


                }
                else
                {
                    ds = null;


                }
            }
            else
            {
                ds = null;

            }
            Int16 i = 1;
            foreach (var dataSet in ds)
            {
                // Duyệt qua từng DataTable trong DataSet
                foreach (DataTable table in dataSet.Tables)
                {
                    string dz = "d" + i.ToString();

                    // Tên DataSet trong RDLC phải khớp với "YourDataSetName"
                    ReportDataSource dataSource = new ReportDataSource(dz, table);
                    report.DataSources.Add(dataSource);
                    i++;
                }
            }


           
            //// Thiết lập tham số nếu có
            //if (parameters.Parameters != null)
            //{
            //    List<ReportParameter> reportParameters = new List<ReportParameter>();
            //    foreach (var param in parameters.Parameters)
            //    {
            //        reportParameters.Add(new ReportParameter(param.Key, param.Value.ToString()));
            //    }
            //    report.SetParameters(reportParameters);
            //}

         
            // Tạo stream từ PDF
            var pdfStream = new MemoryStream();
            byte[] pdfBytes = report.Render("PDF");
            pdfStream.Write(pdfBytes, 0, pdfBytes.Length);
            pdfStream.Position = 0; // Đặt con trỏ về đầu stream

            // Trả về stream dưới dạng FileStreamResult
            //return File(pdfStream, "application/pdf", "Report.pdf");
            return File(pdfStream, "application/pdf", sysID + ".pdf");
            //return Ok(File(pdfStream, "application/pdf", "Report.pdf"));
        }
        catch (Exception ex)
        {
            _logger.Error("Lỗi khi in pdf " + sysID + ".", ex);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("exportpdfFR")]
    public async Task<IActionResult> ExportPdfFR([FromQuery] string sysID, [FromBody] Dictionary<string, object> requestData)
    {
        var filterString = "";
        FastReport.Utils.RegisteredObjects.AddConnection(typeof(FastReport.Data.MsSqlDataConnection));

        FastReport.Report report = new FastReport.Report();
        try
        {
            // Đường dẫn đến tệp báo cáo RDLC

            string baseDirectory = Directory.GetCurrentDirectory();
            string reportPath = Path.Combine(baseDirectory, "Controllers", "FastReport", sysID + ".frx");
            if (!System.IO.File.Exists(reportPath))
            {
                return NotFound(new { message = "Report file not found." });
            }

            

            // Thiết lập nguồn dữ liệu cho báo cáo
            ReportFilter reportFilter = new ReportFilter();
            var filtersFromXML = reportFilter.GetFiltersFromXml(sysID);
            var storeFromXML = reportFilter.GetStoreProcName(sysID);
            var response = new ReportResponse();
            // Truy xuất thông tin từ requestData
            var ds = new List<DataSet>();
            if (requestData.ContainsKey("filters"))
            {
                var filtersObj = requestData["filters"];
                if (filtersObj is JsonElement jsonElement)
                {
                    var processedFilters = new Dictionary<string, object>();
                    var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.ToString());
                    if (filters != null)
                    {
                        // Lặp qua các filter từ XML và kiểm tra trong requestData
                        foreach (var xmlFilter in filtersFromXML)
                        {
                            // Kiểm tra xem filter có trong requestData không
                            if (filters.ContainsKey(xmlFilter.ID))
                            {
                                // Lấy giá trị từ requestData
                                var filterValue = filters[xmlFilter.ID];

                                // Xử lý dữ liệu theo loại (type) từ file XML
                                if (xmlFilter.Type == "date" && DateTime.TryParse(filterValue.ToString(), out var dateValue))
                                {
                                    string formattedFromDate = dateValue.ToString("yyyyMMdd");
                                    processedFilters[xmlFilter.ID] = formattedFromDate;
                                }
                                else if (xmlFilter.Type == "int" && int.TryParse(filterValue.ToString(), out var intValue))
                                {
                                    processedFilters[xmlFilter.ID] = intValue;
                                }
                                else if (xmlFilter.Type == "numeric" && decimal.TryParse(filterValue.ToString(), out var numValue))
                                {
                                    processedFilters[xmlFilter.ID] = numValue;
                                }
                                else
                                {
                                    processedFilters[xmlFilter.ID] = filterValue; // Nếu không có loại, dùng trực tiếp giá trị
                                }
                            }
                        }
                    }
                    // Lấy thông tin phân trang
                    var currentPage = 1;
                    var pageSize = 10;

                    // Kiểm tra xem 'pagination' có tồn tại trong requestData không
                    if (requestData.ContainsKey("pagination"))
                    {
                        var paginationObj = requestData["pagination"];

                        // Kiểm tra xem paginationObj có phải là JsonElement không
                        if (paginationObj is JsonElement paginationElement)
                        {
                            // Deserialize JsonElement thành Dictionary<string, object>
                            var pagination = JsonSerializer.Deserialize<Dictionary<string, object>>(paginationElement.ToString());

                            if (pagination != null)
                            {
                                // Lấy giá trị currentPage nếu có, mặc định là 1
                                if (pagination.ContainsKey("currentPage"))
                                {
                                    // Trích xuất giá trị currentPage từ JsonElement và chuyển sang int
                                    currentPage = Convert.ToInt32(pagination["currentPage"].ToString());
                                }

                                // Lấy giá trị pageSize nếu có, mặc định là 10
                                if (pagination.ContainsKey("pageSize"))
                                {
                                    // Trích xuất giá trị pageSize từ JsonElement và chuyển sang int
                                    pageSize = Convert.ToInt32(pagination["pageSize"].ToString());
                                }
                            }
                        }
                    }
                    //thêm tham số user (để phân quyền trong db)
                    var userOje = JsonSerializer.Deserialize<Dictionary<string, object>>(requestData["user"].ToString());
                    var userID = userOje["userId"].ToString();
                    processedFilters["userId"] = userID;
                    // Thêm tham số phân trang
                    processedFilters["currentPage"] = 0;
                    processedFilters["pageSize"] = 0;

                    // Lấy dữ liệu báo cáo từ repository

                     filterString = string.Join(", ", processedFilters.Values.Select(v => $"\'{v}\'"));
                    filterString = "exec " + storeFromXML + " " + filterString;

                    // Tạo đối tượng Report
                    

                    //ds = await _service.getDataSetReport(filterString);
                    //string xmlStruct = Path.Combine(baseDirectory, "Controllers", "FastReport", sysID + ".xsd");
                    //ds[0].WriteXmlSchema(xmlStruct);
                    //var response = new
                    //{
                    //    Status = "1",
                    //    Message = "Data processed successfully",
                    //    Reports = reports
                    //};


                }
                else
                {
                    ds = null;


                }
            }
            else
            {
                ds = null;

            }        

            report.Load(reportPath);
            var connections = report.Dictionary.Connections;
            var connection = connections[0];

            if (connection == null)
            {
                // Trả về danh sách các kết nối hiện có để dễ dàng kiểm tra
                return BadRequest(new { message = "MsSqlDataConnection 'Connection' not found in report." });
            }

            // Đổi động ConnectionString
           // connection.ConnectionString = request.ConnectionString;

            // Đổi động SelectCommand
            connection.Tables[0].SelectCommand = filterString;

            // Đăng ký DataSet vào báo cáo
            //report.RegisterData(dataSet.Tables[0]);
            //report.GetDataSource("Table").Enabled = true;


            // Xử lý báo cáo
            report.Prepare();

            // Xuất báo cáo sang PDF
            using (MemoryStream ms = new MemoryStream())
            {
                //PDFSimpleExport pdfExport = new PDFSimpleExport();
                //report.Export(pdfExport, ms);
                //byte[] pdfBytes = ms.ToArray();
                //return File(pdfBytes, "application/pdf", "SalesReport.pdf");

                FastReport.Export.Pdf.PDFExport pdfExport = new FastReport.Export.Pdf.PDFExport();
                pdfExport.ShowProgress = false;
                pdfExport.Subject = sysID;
                pdfExport.Title = sysID;
                pdfExport.Compressed = true;
                pdfExport.AllowPrint = true;
                pdfExport.EmbeddingFonts = true;

                MemoryStream strm = new MemoryStream();
                report.Report.Export(pdfExport, strm);
                report.Dispose();
                pdfExport.Dispose();
                strm.Position = 0;

                // return stream in browser
                return File(strm, "application/pdf", sysID+".pdf");
            }

            // Tạo stream từ PDF
            //var pdfStream = new MemoryStream();
            //byte[] pdfBytes = report.Render("PDF");
            //pdfStream.Write(pdfBytes, 0, pdfBytes.Length);
            //pdfStream.Position = 0; // Đặt con trỏ về đầu stream

            //// Trả về stream dưới dạng FileStreamResult
            ////return File(pdfStream, "application/pdf", "Report.pdf");
            //return File(pdfStream, "application/pdf", sysID + ".pdf");
            //return Ok(File(pdfStream, "application/pdf", "Report.pdf"));
        }
        catch (Exception ex)
        {
            _logger.Error("Lỗi khi in pdf " + sysID + ".", ex);
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost("exportpdfFRV2")]
    public async Task<IActionResult> exportpdfFRV2([FromQuery] string sysID, [FromBody] Dictionary<string, object> requestData)
    {
        var filterString = "";
        FastReport.Utils.RegisteredObjects.AddConnection(typeof(FastReport.Data.MsSqlDataConnection));

        FastReport.Report report = new FastReport.Report();
        var ds = new DataSet();
        try
        {
            // Đường dẫn đến tệp báo cáo RDLC

            string baseDirectory = Directory.GetCurrentDirectory();
            string reportPath = Path.Combine(baseDirectory, "Controllers", "FastReport", sysID + ".frx");
           



            // Thiết lập nguồn dữ liệu cho báo cáo
            ReportFilter reportFilter = new ReportFilter();
            var filtersFromXML = reportFilter.GetFiltersFromXml(sysID);
            var storeFromXML = reportFilter.GetStoreProcName(sysID);
            var response = new ReportResponse();
            // Truy xuất thông tin từ requestData
           
            if (requestData.ContainsKey("filters"))
            {
                var filtersObj = requestData["filters"];
                if (filtersObj is JsonElement jsonElement)
                {
                    var processedFilters = new Dictionary<string, object>();
                    var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.ToString());
                    if (filters != null)
                    {
                        // Lặp qua các filter từ XML và kiểm tra trong requestData
                        foreach (var xmlFilter in filtersFromXML)
                        {
                            // Kiểm tra xem filter có trong requestData không
                            if (filters.ContainsKey(xmlFilter.ID))
                            {
                                // Lấy giá trị từ requestData
                                var filterValue = filters[xmlFilter.ID];

                                // Xử lý dữ liệu theo loại (type) từ file XML
                                if (xmlFilter.Type == "date" && DateTime.TryParse(filterValue.ToString(), out var dateValue))
                                {
                                    string formattedFromDate = dateValue.ToString("yyyyMMdd");
                                    processedFilters[xmlFilter.ID] = formattedFromDate;
                                }
                                else if (xmlFilter.Type == "int" && int.TryParse(filterValue.ToString(), out var intValue))
                                {
                                    processedFilters[xmlFilter.ID] = intValue;
                                }
                                else if (xmlFilter.Type == "numeric" && decimal.TryParse(filterValue.ToString(), out var numValue))
                                {
                                    processedFilters[xmlFilter.ID] = numValue;
                                }
                                else
                                {
                                    processedFilters[xmlFilter.ID] = filterValue; // Nếu không có loại, dùng trực tiếp giá trị
                                }
                            }
                        }
                    }
                    // Lấy thông tin phân trang
                    var currentPage = 1;
                    var pageSize = 10;

                    // Kiểm tra xem 'pagination' có tồn tại trong requestData không
                    if (requestData.ContainsKey("pagination"))
                    {
                        var paginationObj = requestData["pagination"];

                        // Kiểm tra xem paginationObj có phải là JsonElement không
                        if (paginationObj is JsonElement paginationElement)
                        {
                            // Deserialize JsonElement thành Dictionary<string, object>
                            var pagination = JsonSerializer.Deserialize<Dictionary<string, object>>(paginationElement.ToString());

                            if (pagination != null)
                            {
                                // Lấy giá trị currentPage nếu có, mặc định là 1
                                if (pagination.ContainsKey("currentPage"))
                                {
                                    // Trích xuất giá trị currentPage từ JsonElement và chuyển sang int
                                    currentPage = Convert.ToInt32(pagination["currentPage"].ToString());
                                }

                                // Lấy giá trị pageSize nếu có, mặc định là 10
                                if (pagination.ContainsKey("pageSize"))
                                {
                                    // Trích xuất giá trị pageSize từ JsonElement và chuyển sang int
                                    pageSize = Convert.ToInt32(pagination["pageSize"].ToString());
                                }
                            }
                        }
                    }
                    //thêm tham số user (để phân quyền trong db)
                    var userOje = JsonSerializer.Deserialize<Dictionary<string, object>>(requestData["user"].ToString());
                    var userID = userOje["userId"].ToString();
                    processedFilters["userId"] = userID;
                    // Thêm tham số phân trang
                    processedFilters["currentPage"] = 0;
                    processedFilters["pageSize"] = 0;

                    // Lấy dữ liệu báo cáo từ repository

                    filterString = string.Join(", ", processedFilters.Values.Select(v => $"\'{v}\'"));
                    filterString = "exec " + storeFromXML + " " + filterString;

                    // Tạo đối tượng Report


                    ds = await _service.getDataSetSystemReport(filterString);
                    //string xmlStruct = Path.Combine(baseDirectory, "Controllers", "FastReport", sysID + ".xsd");
                    //ds[0].WriteXmlSchema(xmlStruct);
                    //var response = new
                    //{
                    //    Status = "1",
                    //    Message = "Data processed successfully",
                    //    Reports = reports
                    //};


                }
                else
                {
                    ds = null;


                }
            }
            else
            {
                ds = null;

            }

            report.RegisterData(ds, "MyDataSet");

            // Kích hoạt tất cả các DataTable trong DataSet
            foreach (DataTable dt in ds.Tables)
            {
                var dataSource = report.GetDataSource(dt.TableName);
                if (dataSource != null)
                    dataSource.Enabled = true;
            }


            if (!System.IO.File.Exists(reportPath))
            {
                report.Save(reportPath);

            }
            report.Load(reportPath);
            // Đăng ký DataSet với báo cáo (tên "MyDataSet" sẽ hiển thị trong Data Dictionary)
            
            // Xử lý báo cáo
            report.Prepare();

            // Xuất báo cáo sang PDF
            using (MemoryStream ms = new MemoryStream())
            {
                //PDFSimpleExport pdfExport = new PDFSimpleExport();
                //report.Export(pdfExport, ms);
                //byte[] pdfBytes = ms.ToArray();
                //return File(pdfBytes, "application/pdf", "SalesReport.pdf");

                FastReport.Export.Pdf.PDFExport pdfExport = new FastReport.Export.Pdf.PDFExport();
                pdfExport.ShowProgress = false;
                pdfExport.Subject = sysID;
                pdfExport.Title = sysID;
                pdfExport.Compressed = true;
                pdfExport.AllowPrint = true;
                pdfExport.EmbeddingFonts = true;

                MemoryStream strm = new MemoryStream();
                report.Report.Export(pdfExport, strm);
                report.Dispose();
                pdfExport.Dispose();
                strm.Position = 0;

                // return stream in browser
                return File(strm, "application/pdf", sysID + ".pdf");
            }

            // Tạo stream từ PDF
            //var pdfStream = new MemoryStream();
            //byte[] pdfBytes = report.Render("PDF");
            //pdfStream.Write(pdfBytes, 0, pdfBytes.Length);
            //pdfStream.Position = 0; // Đặt con trỏ về đầu stream

            //// Trả về stream dưới dạng FileStreamResult
            ////return File(pdfStream, "application/pdf", "Report.pdf");
            //return File(pdfStream, "application/pdf", sysID + ".pdf");
            //return Ok(File(pdfStream, "application/pdf", "Report.pdf"));
        }
        catch (Exception ex)
        {
            _logger.Error("Lỗi khi in pdf " + sysID + ".", ex);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("SetReportData")]
    public async Task<ReportResponse> SetReportData([FromQuery] string sysID, [FromBody] Dictionary<string, object> requestData)
    {
        var response = new ReportResponse();

        try
        {
            // Khởi tạo ReportFilter và lấy các filter từ XML
            ReportFilter reportFilter = new ReportFilter();
            var filtersFromXML = reportFilter.GetFiltersFromXml(sysID);
            var storeProcName = reportFilter.GetStoreProcName(sysID);

            // Kiểm tra sự tồn tại của 'filters' trong requestData
            if (requestData.ContainsKey("filters"))
            {
                var filtersObj = requestData["filters"];
                if (filtersObj is JsonElement jsonElement)
                {
                    var processedFilters = new Dictionary<string, object>();
                    var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.ToString());

                    if (filters != null)
                    {
                        // Lặp qua các filter từ XML và kiểm tra trong requestData
                        foreach (var xmlFilter in filtersFromXML)
                        {
                            if (filters.ContainsKey(xmlFilter.ID))
                            {
                                var filterValue = filters[xmlFilter.ID];
                                object processedValue = null;

                                if (filterValue is JsonElement jsonFilterElement)
                                {
                                    processedValue = jsonFilterElement.GetValue(xmlFilter.Type);
                                }
                                else
                                {
                                    // Nếu filterValue không phải là JsonElement, sử dụng trực tiếp
                                    processedValue = filterValue ?? DBNull.Value;
                                }

                                processedFilters[xmlFilter.ID] = processedValue;
                            }
                        }
                    }

                    // Lấy thông tin phân trang
                    var currentPage = 1;
                    var pageSize = 10;

                    if (requestData.ContainsKey("pagination"))
                    {
                        var paginationObj = requestData["pagination"];
                        if (paginationObj is JsonElement paginationElement)
                        {
                            var pagination = JsonSerializer.Deserialize<Dictionary<string, object>>(paginationElement.ToString());
                            if (pagination != null)
                            {
                                if (pagination.ContainsKey("currentPage") && int.TryParse(pagination["currentPage"].ToString(), out var cp))
                                {
                                    currentPage = cp;
                                }

                                if (pagination.ContainsKey("pageSize") && int.TryParse(pagination["pageSize"].ToString(), out var ps))
                                {
                                    pageSize = ps;
                                }
                            }
                        }
                    }

                    // Lấy thông tin user
                    int userID = 0;
                    if (requestData.ContainsKey("user"))
                    {
                        var userObj = requestData["user"];
                        if (userObj is JsonElement userElement)
                        {
                            var userDict = JsonSerializer.Deserialize<Dictionary<string, object>>(userElement.ToString());
                            if (userDict != null && userDict.ContainsKey("userId") && int.TryParse(userDict["userId"].ToString(), out var uid))
                            {
                                userID = uid;
                                processedFilters["userId"] = userID;
                            }
                        }
                    }

                    // Thêm tham số phân trang
                    processedFilters["currentPage"] = currentPage;
                    processedFilters["pageSize"] = pageSize;

                    // Thực thi stored procedure và lấy dữ liệu
                    response = await _service.SetReportData(sysID, storeProcName, userID, currentPage, pageSize, processedFilters);
                }
                else
                {
                    // Nếu 'filters' không phải là JsonElement hợp lệ
                    response.Status = "0";
                    response.Message = "'filters' should be a valid JSON object.";
                    response.Reports = null;
                    response.MasterData = null;
                }
            }
            else
            {
                // Nếu thiếu 'filters' trong requestData
                response.Status = "0";
                response.Message = "Missing 'filters' in the request data.";
                response.Reports = null;
                response.MasterData = null;
            }
        }
        catch (Exception ex)
        {
            // Xử lý lỗi và trả về phản hồi lỗi
            _logger.Error("Lỗi khi lấy dữ liệu " + sysID + ".", ex);
            response.Status = "0";
            response.Message = $"An error occurred: {ex.Message}";
            response.Reports = null;
            response.MasterData = null;
        }

        return response;
    }

    [HttpPost("sendemail")]
    public async Task<IActionResult> SendEmail([FromBody] EmailModel model)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", model.To));
            message.Subject = model.Subject;

            var builder = new BodyBuilder();
            builder.TextBody = model.Body;

            // Thêm file PDF đính kèm
            if (!string.IsNullOrEmpty(model.AttachmentData))
            {
                var pdfBytes = Convert.FromBase64String(model.AttachmentData);
                builder.Attachments.Add("report.pdf", pdfBytes, ContentType.Parse("application/pdf"));
            }

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // Tắt xác thực chứng chỉ SSL
                client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

                // Kết nối với STARTTLS
                await client.ConnectAsync(
                    _emailSettings.SmtpServer,
                    _emailSettings.Port,
                    SecureSocketOptions.StartTls
                );

                // Xác thực
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);

                // Gửi email
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return Ok(new { success = true, message = "Email sent successfully" });
        }
        catch (Exception ex)
        {
            // Log chi tiết lỗi
            var errorMessage = $"Failed to send email: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
            }
            return BadRequest(new { success = false, message = errorMessage });
        }
    }

}
