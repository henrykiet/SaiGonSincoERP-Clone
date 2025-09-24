using reportSystem01.Shared;
using Sinco.Server.Helpers;
using Sinco.Server.Repositories.BaseRepository;
using Sinco.Server.SqlJsonDefinations.Requests;
using Sinco.Server.SqlJsonDefinations.Responses;
using System.Dynamic;
using System.Text.Json;

namespace Sinco.Server.Repositories
{
    public interface IDataService
    {
        Task<ServiceResponse<MetadataResponse>> GetMetadata(string controller, MetadataRequest request);
        Task<DataListResponse> GetDataList(DataListRequest request);
        Task<DataSaveResponse> PostDataSave(DataSaveRequest request);
        Task<DataDeleteResponse> DeleteData(DataDeleteRequest request);
        Task<ServiceResponse<object>> DeletedMultiData(DataDeleteRequest request);
        Task<List<DataOptionsResponse>> GetDataOptions(string optionId, DataOptionsRequest request);
        Task<List<DataLookupResponse>> GetDataLookup(string formId, DataLookupRequest request);
        Task<List<DataReportResponse>> GetDataReport(string formId, DataReportRequest request);
        Task<DataReportPivotResponse> GetDataReportPivot(string foormId, DataReportPivotRequest request);
        Task<DataCustomResponse> PostDataCustom(DataCustomRequest request);
    }
    public class DataService : IDataService
    {
        private readonly IBaseRepository<ExpandoObject> _repository;
        public DataService(IBaseRepository<ExpandoObject> repository)
        {
            _repository = repository;
        }
        #region Get Metadata
        public async Task<ServiceResponse<MetadataResponse>> GetMetadata(string controller, MetadataRequest request)
        {
            var response = new ServiceResponse<MetadataResponse>();
            var metadata = new MetadataResponse();
            var tabs = new List<MetadataResponse.Tab>();
            string tableName = request.FormId.Trim();
            try
            {
                //đọc json ra và lấy tên filed và cho vào getById
                var jsonDefault = await _repository.GetTemplateMetadataByTableNameAsync($"{controller}.page");
                if (jsonDefault != null)
                {
                    var jsonMatch = jsonDefault.FormId.Trim().ToLower().Equals(request.FormId.Trim().ToLower(), StringComparison.OrdinalIgnoreCase);
                    if (jsonMatch)
                    {
                        //lọc tab theo formId tab trùng với formId của metedata để gán gí trị init vào nếu có update
                        var jsonTab = jsonDefault.Tabs.FirstOrDefault(t => t.Form.FormId.Trim().ToLower().Equals(jsonDefault.FormId.Trim().ToLower(), StringComparison.OrdinalIgnoreCase));
                        //nếu ko có tab thì trả về form rỗng
                        if (jsonTab != null)
                        {
                            if (request.Action.ToLower().Trim() == "update" && !string.IsNullOrEmpty(request.Primarykey))
                            {
                                var isUpdate = true;
                                //trường hợp update thì fill các value vào fields và initial
                                dynamic? data = null;
                                Dictionary<string, string>? keyDict = null;
                                string json = request.Primarykey!.Trim();
                                try
                                {
                                    // Nếu Primarykey là chuỗi JSON (khóa kép)
                                    // TH1: Nếu là chuỗi JSON bình thường (object): {"maDonHang":"DH1",...}
                                    if (json.StartsWith("{"))
                                    {
                                        keyDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                                    }
                                    // TH2: Nếu là chuỗi JSON bên trong một chuỗi (escaped): "{\"maDonHang\":\"DH1\",...}"
                                    else if (json.StartsWith("\""))
                                    {
                                        // Bỏ dấu " bao quanh, rồi deserialize chuỗi bên trong
                                        var unescapedJson = JsonSerializer.Deserialize<string>(json);
                                        keyDict = JsonSerializer.Deserialize<Dictionary<string, string>>(unescapedJson!);
                                    }
                                    if (keyDict != null)
                                    {
                                        data = await _repository.GetByIdAsync(keyDict, tableName, null);
                                    }
                                    else
                                    {
                                        // Khóa đơn
                                        data = await _repository.GetByIdAsync(request.Primarykey!, tableName, null);
                                    }
                                    if (data is IEnumerable<dynamic> list)
                                        data = list.FirstOrDefault();
                                }
                                catch (JsonException)
                                {
                                    response.Success = false;
                                    response.Message = "Lỗi định dạng khóa chính (chuỗi JSON không hợp lệ).";
                                    return response;
                                }
                                if (data != null)
                                {
                                    MetadataResponse.Tab newTab = await BuildMainTab(tableName, data, jsonTab, includeInitial: isUpdate);
                                    if (newTab == null)
                                    {
                                        response.StatusCode = 400;
                                        response.Success = true;
                                        response.Message = $"Error when build tab: not found {tableName}";
                                        return response;
                                    }
                                    metadata.PrimaryKey = newTab.Form.PrimaryKey;
                                    //lấy dữ liệu detail
                                    //nếu json có detail thì mới thực hiện
                                    if (jsonTab.Detail != null)
                                    {
                                        MetadataResponse.Tab.TabDetail? newDetailTab = await BuildTabDetail(tableName, newTab.Form.PrimaryKey, jsonTab, includeInitial: isUpdate);
                                        if (newDetailTab != null)
                                        {
                                            newTab.Detail = newDetailTab;
                                        }
                                    }
                                    tabs.Add(newTab);
                                    response.Success = true;
                                    response.Message = "Form entity";
                                }
                                //không có entity
                                else
                                {
                                    response.Success = false;
                                    response.Message = $"Not found {request.Primarykey} in {tableName}";
                                }
                            }
                            //trường hợp có action update nhưng không có primarykey
                            else if (request.Action.ToLower().Trim() == "update" && string.IsNullOrEmpty(request.Primarykey))
                            {
                                response.Success = false;
                                response.Message = "Not have primarykey to update";
                            }
                            //trường hợp insert chỉ cần show 1 form dựa theo formId
                            else if (request.Action.ToLower().Trim() == "insert")
                            {
                                var tab = jsonDefault.Tabs.FirstOrDefault(t => t.Form.FormId.Trim().ToLower().Equals(request.FormId.Trim().ToLower(), StringComparison.OrdinalIgnoreCase));
                                if (tab != null)
                                    tabs.Add(tab);
                                response.Message = "Metadata insert";
                            }
                            //trường hợp trả về list bảng theo mẫu nếu ko có action
                            else
                            {
                                tabs.AddRange(jsonDefault.Tabs);
                                response.Message = "Not have action";
                            }
                        }
                        else
                        {
                            //form id không trùng với bất kỳ tab nào thì trả về form mẫu trong file json	
                            tabs.AddRange(jsonDefault.Tabs);
                            response.Message = "Metadata not match tab";
                        }
                    }
                    else
                    {
                        //trường hợp formId không trùng
                        response.Message = "FormId not match";
                    }
                    metadata.Controller = tableName;
                    metadata.FormId = jsonDefault.FormId;
                    metadata.Type = jsonDefault.Type;
                    metadata.Title = jsonDefault.Title;
                    metadata.IdVC = jsonDefault.IdVC;
                    metadata.VCDate = jsonDefault.VCDate;
                    metadata.Class = jsonDefault.Class;
                    metadata.PrimaryKey ??= request.Primarykey!;
                    metadata.DataProcessing = new MetadataResponse.DataProcess
                    {
                        Actions = new MetadataResponse.DataProcess.Action
                        {
                            Posts = new List<MetadataResponse.DataProcess.Action.Post>
                                {
                                    new MetadataResponse.DataProcess.Action.Post
                                    {
                                        Step = "soKho",
                                        Type = "sql",
                                        Query = "INSERT INTO KhachHang (CustomerCode, CustomerName, Phone) VALUES (@CustomerCode, @CustomerName, @Phone)"
                                    },
                                    new MetadataResponse.DataProcess.Action.Post
                                    {
                                        Step = "soCai",
                                        Type = "sql",
                                        Query = "INSERT INTO Logs (Action, Description) VALUES ('Insert', 'Thêm mới khách hàng')"
                                    }
                                }
                        }
                    };
                    metadata.Tabs = tabs;
                    response.Data = metadata;
                }
                else
                {
                    response.Success = false;
                    response.Message = $"{controller} not exit!";
                }
                return await Task.FromResult(response);
            }
            catch (ExceptionFormat ex)
            {
                response.Success = false;
                response.Message = string.Join("; ", ex.Errors);
                return response;
            }
        }
        private async Task<MetadataResponse.Tab?> BuildMainTab(string tableName, IDictionary<string, object> values, MetadataResponse.Tab? jsonTab, bool includeInitial = true)
        {
            var fields = new List<MetadataResponse.Tab.TabForm.Field>();
            var initial = new Dictionary<string, object>();
            var sqlDef = await _repository.GetTemplateByTableNameAsync(tableName);
            //tìm khóa chính
            var pkFields = sqlDef?.Schema.Fields.Where(f => f.PrimaryKey == true).Select(f => f.Name).ToList();
            if (pkFields == null || !pkFields.Any()) return null;
            var keyValues = pkFields
                .Select(pk => values.ContainsKey(pk) ? values[pk]?.ToString() ?? "" : "")
                .ToList();
            var stringPrimaryKey = string.Join(",", keyValues);

            foreach (var defField in jsonTab?.Form?.Fields ?? new List<MetadataResponse.Tab.TabForm.Field>())
            {
                //chỉ lấy field trùng với json
                var key = defField.Key;
                values.TryGetValue(key, out var value);

                if (includeInitial && value != null)
                {
                    initial[key] = value;
                }

                string? placeholder = value switch
                {
                    null => null,
                    DateTime dt => dt.ToString("yyyy-MM-dd"),
                    decimal dec => dec.ToString("0.##"),
                    _ => value?.ToString()
                };

                fields.Add(new MetadataResponse.Tab.TabForm.Field
                {
                    Key = key,
                    Label = defField.Label,
                    Type = defField.Type,
                    Required = defField.Required,
                    Placeholder = placeholder,
                });
            }
            return new MetadataResponse.Tab
            {
                Title = jsonTab?.Title ?? tableName,
                Form = new MetadataResponse.Tab.TabForm
                {
                    TypeForm = "master",
                    FormId = tableName,
                    PrimaryKey = stringPrimaryKey,
                    Title = jsonTab?.Form?.Title ?? $"Thông tin {tableName}",
                    Fields = fields,
                    InitialData = initial
                }
            };
        }

        private async Task<MetadataResponse.Tab.TabDetail?> BuildTabDetail(string tableName, string keyMainTable, MetadataResponse.Tab? jsonTab, bool includeInitial = true)
        {
            var fields = new List<MetadataResponse.Tab.TabDetail.FieldDetail>();
            var initials = new List<Dictionary<string, object>>();

            var allDefs = await _repository.GetAllTemplateDefinitionsAsync();
            //tìm bảng phụ dựa vào việc so sánh tên bảng chính
            var detailDef = allDefs.FirstOrDefault(def =>
                                    def?.Schema?.Fields?.Any(f =>
                                    f?.Foreign?.Table?.Equals(tableName, StringComparison.OrdinalIgnoreCase) == true) == true);
            if (detailDef == null) return null;
            //tìm field khóa ngoại của bảng phụ nối với bảng chính
            var foreign = detailDef?.Schema.Fields.FirstOrDefault(f =>
                f.Foreign?.Table?.Equals(tableName, StringComparison.OrdinalIgnoreCase) == true);
            if (foreign == null) return null;

            var detailDatas = await _repository.GetByIdAsync(keyMainTable, detailDef!.Model, foreign.Foreign!.Key);
            if (detailDatas is not IEnumerable<dynamic> list) list = new List<dynamic> { detailDatas };

            if (jsonTab?.Detail?.Fields != null)
            {
                foreach (var field in jsonTab.Detail.Fields)
                {
                    fields.Add(new MetadataResponse.Tab.TabDetail.FieldDetail
                    {
                        Key = field.Key,
                        Label = field.Label,
                        Type = field.Type,
                        Placeholder = field.Placeholder,
                        Disabled = field.Disabled
                    });
                }
            }

            if (includeInitial)
            {
                if (list != null)
                {
                    foreach (var row in list)
                    {
                        var dict = (IDictionary<string, object>)row;
                        initials.Add(new Dictionary<string, object>(dict));
                    }
                }
            }

            var primaryKey = detailDef!.Schema.Fields.FirstOrDefault(f => f.PrimaryKey == true)?.Name ?? "Id";

            return new MetadataResponse.Tab.TabDetail
            {
                Title = jsonTab?.Detail?.Title ?? $"Chi tiết {tableName}",
                TypeForm = "detail",
                FormId = detailDef.Model,
                ForeignKey = foreign.Foreign.Key,
                PrimaryKey = primaryKey,
                Entity = "",
                Fields = fields,
                InitialDatas = initials
            };
        }
        #endregion
        public Task<DataListResponse> GetDataList(DataListRequest request)
        {
            var datas = new List<DataListResponse.Data>();
            datas.Add(new DataListResponse.Data() { OrderCode = "orderCode1", OrderDate = DateTime.Now, Status = "active" });
            datas.Add(new DataListResponse.Data() { OrderCode = "orderCode2", OrderDate = DateTime.Now, Status = "unactive" });
            var result = new DataListResponse()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Datas = datas,
                Total = 10
            };
            return Task.FromResult(result);
        }

        public Task<DataSaveResponse> PostDataSave(DataSaveRequest request)
        {
            var response = new DataSaveResponse
            {
                Success = true,
                Message = "Dữ liệu đã được lưu thành công.",
                Errors = new List<DataSaveResponse.Error>()
            };

            return Task.FromResult(response);
        }
        public Task<DataDeleteResponse> DeleteData(DataDeleteRequest request)
        {
            var response = new DataDeleteResponse
            {
                Success = true,
                Message = "Dữ liệu đã được xóa thành công."
            };

            return Task.FromResult(response);
        }
        public async Task<ServiceResponse<object>> DeletedMultiData(DataDeleteRequest request)
        {
            var response = new ServiceResponse<object>();
            if (request.Action != null && request.Action.ToLower().Trim() != "delete")
            {
                response.Success = false;
                response.Message = "Action must be delete";
                return response;
            }
            try
            {
                var deletedResult = await _repository.DeleteMultiAsync(request.Ids, request.FormId, request.Status, request.PrimaryKey);
                if (deletedResult > 0)
                {
                    response.Success = true;
                    response.Message = $"Deleted {deletedResult} records successfully.";
                }
                else
                {
                    response.Success = false;
                    response.Message = "No records were deleted.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error when delete multi data: {ex.Message}";
                return response;
            }

            return response;
        }

        public Task<List<DataOptionsResponse>> GetDataOptions(string optionId, DataOptionsRequest request)
        {
            var options = new List<DataOptionsResponse>
                        {
                            new DataOptionsResponse { Id = "1", Name = "Option 1" },
                            new DataOptionsResponse { Id = "2", Name = "Option 2" }
                        };
            return Task.FromResult(options);
        }
        public Task<List<DataLookupResponse>> GetDataLookup(string formId, DataLookupRequest request)
        {
            var lookups = new List<DataLookupResponse>
                        {
                            new DataLookupResponse { VoucherCode = "VC001", Date = DateTime.Now, Amount = 1000 },
                            new DataLookupResponse { VoucherCode = "VC002", Date = DateTime.Now.AddDays(-1), Amount = 2000 }
                        };
            return Task.FromResult(lookups);
        }

        public Task<List<DataReportResponse>> GetDataReport(string formId, DataReportRequest request)
        {
            var reports = new List<DataReportResponse>
                        {
                            new DataReportResponse { OrderCode = "ORD001", TotalAmount = 5000, OrderDate = DateTime.Today },
                            new DataReportResponse { OrderCode = "ORD002", TotalAmount = 3000, OrderDate = DateTime.Today.AddDays(-2) }
                        };
            return Task.FromResult(reports);
        }

        public Task<DataReportPivotResponse> GetDataReportPivot(string foormId, DataReportPivotRequest request)
        {
            var response = new DataReportPivotResponse
            {
                Rows = new List<DataReportPivotResponse.Row>
                        {
                            new DataReportPivotResponse.Row
                            {
                                Month = new DateTime(2024, 1, 1),
                                ProductA = 120,
                                ProductB = 80
                            },
                            new DataReportPivotResponse.Row
                            {
                                Month = new DateTime(2024, 2, 1),
                                ProductA = 150,
                                ProductB = 95
                            }
                        }
            };
            return Task.FromResult(response);
        }

        public Task<DataCustomResponse> PostDataCustom(DataCustomRequest request)
        {
            var response = new DataCustomResponse
            {
                Success = true,
                Result = new DataCustomResponse.DataResult
                {
                    Tax = 10
                }
            };
            return Task.FromResult(response);
        }

    }
}
