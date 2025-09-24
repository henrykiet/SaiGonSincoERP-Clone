using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinco.Server.Models;
using Sinco.Server.Repositories.Dynamic;
using System.Text.Json;
using System.Web;
using reportSystem01.Shared;

namespace Sinco.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicController : ControllerBase
    {
        private readonly IDynamicRepository _dynamicRepository;
        private readonly ILogger<DynamicController> _logger;

        public DynamicController(
            IDynamicRepository dynamicRepository,
            ILogger<DynamicController> logger)
        {
            _dynamicRepository = dynamicRepository;
            _logger = logger;
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<DynamicQueryResponse<Dictionary<string, object>>>> GetList(
            [FromQuery] string formId,
            [FromQuery] string filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sort = null)
        {
            try
            {
                // Decode formId từ URL và xử lý các ký tự đặc biệt
                var decodedFormId = System.Web.HttpUtility.UrlDecode(formId);

                // Parse formId thành dynamic object để lấy các trường
                var formIdObj = JsonSerializer.Deserialize<JsonElement>(decodedFormId);
                var formInfo = new FormInfo();
                var valueList = new List<string>();

                // Lấy các trường từ formIdObj
                foreach (var prop in formIdObj.EnumerateObject())
                {
                    switch (prop.Name.ToLower())
                    {
                        case "controller":
                            formInfo.Controller = prop.Value.GetString();
                            break;
                        case "formid":
                            formInfo.TableName = prop.Value.GetString(); // TableName = formId
                            break;
                        case "tablename":
                            formInfo.TableName = prop.Value.GetString();
                            break;
                        case "primarykey":
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                                formInfo.PrimaryKey = prop.Value.EnumerateArray().Select(x => x.GetString()).ToArray();
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                                formInfo.PrimaryKey = new string[] { prop.Value.GetString() };
                            break;
                        case "value":
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                                valueList = prop.Value.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                                valueList = new List<string> { prop.Value.GetString() ?? "" };
                            break;
                        case "type":
                            formInfo.Type = prop.Value.GetString();
                            break;
                        case "action":
                            formInfo.Action = prop.Value.GetString();
                            break;
                        case "sort":
                            formInfo.Sort = prop.Value.GetString();
                            break;
                        case "language":
                            formInfo.Language = prop.Value.GetString();
                            break;
                        case "unit":
                            formInfo.Unit = prop.Value.GetString();
                            break;
                        case "idvc":
                            formInfo.IdVC = prop.Value.GetString();
                            break;
                        case "userid":
                            formInfo.UserId = prop.Value.GetString();
                            break;
                    }
                }

                // Nếu có sort ngoài query, ưu tiên sort ngoài
                if (!string.IsNullOrEmpty(sort))
                {
                    formInfo.Sort = sort;
                }

                // Nếu TableName chưa có, lấy từ formId
                if (string.IsNullOrEmpty(formInfo.TableName) && formIdObj.TryGetProperty("formId", out var formIdProp))
                {
                    formInfo.TableName = formIdProp.GetString();
                }

                var request = new DynamicQueryRequest
                {
                    FormId = formInfo,
                    Page = page,
                    PageSize = pageSize
                };

                if (!string.IsNullOrEmpty(filter))
                {
                    try
                    {
                        var decodedFilter = System.Web.HttpUtility.UrlDecode(filter);
                        request.Filter = JsonSerializer.Deserialize<JsonElement>(decodedFilter);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning("Failed to deserialize filter: {Error}", ex.Message);
                        // Có thể bỏ qua filter nếu không parse được
                    }
                }

                // Nếu có value, có thể truyền vào request nếu cần dùng ở tầng dưới
                // request.Value = valueList.ToArray(); // Nếu cần truyền xuống repository

                var result = await _dynamicRepository.GetDynamicListAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dynamic list for form {FormId}", formId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("filter")]
        [Authorize]
        public async Task<ActionResult<DynamicQueryResponse<Dictionary<string, object>>>> GetFilteredList(
            [FromQuery] string formId,
            [FromQuery] string filter = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sort = null)
        {
            try
            {
                // Decode formId từ URL và xử lý các ký tự đặc biệt
                var decodedFormId = System.Web.HttpUtility.UrlDecode(formId);

                // Parse formId thành dynamic object để lấy các trường
                var formIdObj = JsonSerializer.Deserialize<JsonElement>(decodedFormId);
                var formInfo = new FormInfo();
                var valueList = new List<string>();

                // Lấy các trường từ formIdObj
                foreach (var prop in formIdObj.EnumerateObject())
                {
                    switch (prop.Name.ToLower())
                    {
                        case "controller":
                            formInfo.Controller = prop.Value.GetString();
                            break;
                        case "formid":
                            formInfo.TableName = prop.Value.GetString(); // TableName = formId
                            break;
                        case "tablename":
                            formInfo.TableName = prop.Value.GetString();
                            break;
                        case "primarykey":
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                                formInfo.PrimaryKey = prop.Value.EnumerateArray().Select(x => x.GetString()).ToArray();
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                                formInfo.PrimaryKey = new string[] { prop.Value.GetString() };
                            break;
                        case "value":
                            if (prop.Value.ValueKind == JsonValueKind.Array)
                                valueList = prop.Value.EnumerateArray().Select(x => x.GetString() ?? "").ToList();
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                                valueList = new List<string> { prop.Value.GetString() ?? "" };
                            break;
                        case "type":
                            formInfo.Type = prop.Value.GetString();
                            break;
                        case "action":
                            formInfo.Action = prop.Value.GetString();
                            break;
                        case "sort":
                            formInfo.Sort = prop.Value.GetString();
                            break;
                        case "language":
                            formInfo.Language = prop.Value.GetString();
                            break;
                        case "unit":
                            formInfo.Unit = prop.Value.GetString();
                            break;
                        case "idvc":
                            formInfo.IdVC = prop.Value.GetString();
                            break;
                        case "userid":
                            formInfo.UserId = prop.Value.GetString();
                            break;
                        case "isfilehandle":
                            formInfo.isFileHandle = prop.Value.GetBoolean();
                            break;
                    }
                }

                // Nếu có sort ngoài query, ưu tiên sort ngoài
                if (!string.IsNullOrEmpty(sort))
                {
                    formInfo.Sort = sort;
                }

                // Nếu TableName chưa có, lấy từ formId
                if (string.IsNullOrEmpty(formInfo.TableName) && formIdObj.TryGetProperty("formId", out var formIdProp))
                {
                    formInfo.TableName = formIdProp.GetString();
                }

                var request = new DynamicFilterRequest
                {
                    FormId = formInfo,
                    Page = page,
                    PageSize = pageSize,
                    Filter = new List<FilterItem>()
                };

                // Parse filter nâng cao với operators
                if (!string.IsNullOrEmpty(filter))
                {
                    try
                    {
                        var decodedFilter = System.Web.HttpUtility.UrlDecode(filter);
                        var filterArray = JsonSerializer.Deserialize<JsonElement>(decodedFilter);
                        
                        if (filterArray.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var filterElement in filterArray.EnumerateArray())
                            {
                                var filterItem = new FilterItem();
                                
                                if (filterElement.TryGetProperty("field", out var fieldProp))
                                    filterItem.Field = fieldProp.GetString();
                                    
                                if (filterElement.TryGetProperty("value", out var valueProp))
                                    filterItem.Value = valueProp.GetString();
                                    
                                if (filterElement.TryGetProperty("operator", out var operatorProp))
                                    filterItem.Operator = operatorProp.GetString();

                                // Validate filter item
                                if (string.IsNullOrEmpty(filterItem.Field))
                                {
                                    _logger.LogWarning("Filter item thiếu trường 'field'");
                                    continue;
                                }
                                
                                if (string.IsNullOrEmpty(filterItem.Operator))
                                {
                                    _logger.LogWarning("Filter item thiếu trường 'operator' cho field '{Field}'", filterItem.Field);
                                    continue;
                                }
                                
                                if (!filterItem.IsValidOperator())
                                {
                                    _logger.LogWarning("Toán tử '{Operator}' không hợp lệ cho field '{Field}'", filterItem.Operator, filterItem.Field);
                                    continue;
                                }

                                request.Filter.Add(filterItem);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Filter phải là một mảng các object");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning("Failed to deserialize filter: {Error}", ex.Message);
                        return BadRequest(new
                        {
                            success = false,
                            message = "Định dạng filter không hợp lệ. Filter phải là mảng các object với cấu trúc: [{\"field\": \"tên_trường\", \"value\": \"giá_trị\", \"operator\": \"toán_tử\"}]",
                            example = new[]
                            {
                                new { field = "status", value = "1", @operator = "=" },
                                new { field = "ten_kh", value = "PhongNN", @operator = "like" }
                            }
                        });
                    }
                }

                var result = await _dynamicRepository.GetDynamicFilterListAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dynamic filtered list for form {FormId}", formId);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        // Method helper để parse FormId manually nếu JSON deserialize không thành công
        private FormInfo ParseFormIdManually(string formIdString)
        {
            try
            {
                // Remove curly braces và split by comma
                formIdString = formIdString.Trim('{', '}');
                var pairs = formIdString.Split(',');

                var formInfo = new FormInfo();
                var primaryKeys = new List<string>();

                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        var key = keyValue[0].Trim().Trim('"');
                        var value = keyValue[1].Trim().Trim('"');

                        switch (key.ToLower())
                        {
                            case "controller":
                                formInfo.Controller = value;
                                break;
                            case "tablename":
                                formInfo.TableName = value;
                                break;
                            case "primarykey":
                                // Nếu value là dạng mảng JSON: ["ma_kh","line_nbr"]
                                if (value.StartsWith("[") && value.EndsWith("]"))
                                {
                                    // Loại bỏ [ ] và tách các phần tử
                                    var keys = value.Trim('[', ']').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim().Trim('"')).ToArray();
                                    primaryKeys.AddRange(keys);
                                }
                                else
                                {
                                    primaryKeys.Add(value);
                                }
                                break;
                            // ... các case khác giữ nguyên ...
                            case "type":
                                formInfo.Type = value;
                                break;
                            case "action":
                                formInfo.Action = value;
                                break;
                            case "sort":
                                formInfo.Sort = value;
                                break;
                            case "language":
                            case "languague":
                                formInfo.Language = value;
                                break;
                            case "unit":
                                formInfo.Unit = value;
                                break;
                            case "idvc":
                                formInfo.IdVC = value;
                                break;
                            case "userid":
                                formInfo.UserId = value;
                                break;
                        }
                    }
                }

                formInfo.PrimaryKey = primaryKeys.ToArray();
                return formInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing formId manually: {FormIdString}", formIdString);
                return null;
            }
        }

        //[HttpGet("{formId}/{id}")]
        //public async Task<ActionResult<Dictionary<string, object>>> GetById(string formId, string id)
        //{
        //    try
        //    {
        //        var result = await _dynamicRepository.GetDynamicByIdAsync(formId, id);
        //        if (result == null)
        //        {
        //            return NotFound();
        //        }
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting dynamic item for form {FormId} with id {Id}", formId, id);
        //        return StatusCode(500, "An error occurred while processing your request");
        //    }
        //}

        [HttpPost("save")]
        [Authorize]
        public async Task<ActionResult<DynamicSaveResponse>> Save([FromBody] DynamicSaveRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(request.FormId))
                {
                    return BadRequest(new DynamicSaveResponse
                    {
                        Success = false,
                        Message = "Mã form không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "formId", Message = "Mã form không được để trống" }
                        }
                    });
                }

                if (string.IsNullOrEmpty(request.Action))
                {
                    return BadRequest(new DynamicSaveResponse
                    {
                        Success = false,
                        Message = "Hành động không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "action", Message = "Hành động không được để trống" }
                        }
                    });
                }

                if (request.PrimaryKey == null || request.PrimaryKey.Length == 0)
                {
                    return BadRequest(new DynamicSaveResponse
                    {
                        Success = false,
                        Message = "Khóa chính không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "primaryKey", Message = "Khóa chính không được để trống" }
                        }
                    });
                }

                var result = await _dynamicRepository.SaveDynamicAsync(request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu dữ liệu động cho form {FormId}", request.FormId);
                return StatusCode(500, new DynamicSaveResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lưu dữ liệu",
                    Errors = new List<ValidationError>
                    {
                        new ValidationError { Field = "system", Message = ex.Message }
                    }
                });
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<ActionResult<DynamicDeleteResponse>> Delete([FromBody] DynamicDeleteRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(request.FormId))
                {
                    return BadRequest(new DynamicDeleteResponse
                    {
                        Success = false,
                        Message = "Mã form không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "formId", Message = "Mã form không được để trống" }
                        }
                    });
                }

                if (string.IsNullOrEmpty(request.Action))
                {
                    return BadRequest(new DynamicDeleteResponse
                    {
                        Success = false,
                        Message = "Hành động không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "action", Message = "Hành động không được để trống" }
                        }
                    });
                }

                if (request.PrimaryKey == null || request.PrimaryKey.Length == 0)
                {
                    return BadRequest(new DynamicDeleteResponse
                    {
                        Success = false,
                        Message = "Khóa chính không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "primaryKey", Message = "Khóa chính không được để trống" }
                        }
                    });
                }

                if (request.Value == null || request.Value.Length == 0)
                {
                    return BadRequest(new DynamicDeleteResponse
                    {
                        Success = false,
                        Message = "Giá trị không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "value", Message = "Giá trị không được để trống" }
                        }
                    });
                }

                if (request.ListTable == null || !request.ListTable.Any())
                {
                    return BadRequest(new DynamicDeleteResponse
                    {
                        Success = false,
                        Message = "Danh sách bảng không được để trống",
                        Errors = new List<ValidationError>
                        {
                            new ValidationError { Field = "listTable", Message = "Danh sách bảng không được để trống" }
                        }
                    });
                }

                var result = await _dynamicRepository.DeleteDynamicAsync(request);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa dữ liệu động cho form {FormId}", request.FormId);
                return StatusCode(500, new DynamicDeleteResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi xóa dữ liệu",
                    Errors = new List<ValidationError>
                    {
                        new ValidationError { Field = "system", Message = ex.Message }
                    }
                });
            }
        }

        [HttpGet("next-field-number")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<string>>> GetNextFieldNumber([FromQuery] string controller, [FromQuery] string field, [FromQuery] string formId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(controller))
                {
                    return BadRequest(ServiceResponse<string>.CreateError("Controller không được để trống", 400));
                }

                if (string.IsNullOrEmpty(field))
                {
                    return BadRequest(ServiceResponse<string>.CreateError("Field không được để trống", 400));
                }

                var result = await _dynamicRepository.GetNextFieldNumberAsync(controller, field, formId);
                
                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy số tiếp theo cho field {field} của controller {controller}");
                return StatusCode(500, ServiceResponse<string>.CreateError("Đã xảy ra lỗi khi lấy số tiếp theo", 500));
            }
        }
    }
} 