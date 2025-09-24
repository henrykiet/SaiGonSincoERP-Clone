using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using reportSystem01.Shared;
using Sinco.Server.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Sinco.Server.Repositories.GetFormData;

namespace Sinco.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormConfigController : ControllerBase
    {
        private readonly IFormConfigRepository _formConfigRepository;
        private readonly ILogger<FormConfigController> _logger;

        public FormConfigController(
            IFormConfigRepository formConfigRepository,
            ILogger<FormConfigController> logger)
        {
            _formConfigRepository = formConfigRepository;
            _logger = logger;
        }

        [HttpPut("{fileName}")]
        //[Authorize]
        public async Task<ActionResult<string>> GetFormConfig(string fileName)
        {
            try
            {
                var content = await _formConfigRepository.GetFormConfigAsync(fileName);
                return Ok(content);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file name format");
                return BadRequest(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Form configuration file not found");
                return NotFound(ex.Message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing form configuration");
                return StatusCode(500, "Error processing form configuration file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting form configuration");
                return StatusCode(500, "An unexpected error occurred");
            }
        }
        [HttpPost("SyncData")]
        //[Authorize]
        public async Task<IActionResult> SyncData([FromBody] SyncRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FormId) || request.Ids == null || request.Ids.Count == 0)
            {
                return BadRequest(new { message = "Thiếu FormId hoặc danh sách Ids." });
            }

            try
            {
                // Đọc form config từ file JSON
                var configContent = await _formConfigRepository.GetFormConfigAsync(request.IdSync);
                var formConfig = JsonConvert.DeserializeObject<dynamic>(configContent);
                if (formConfig == null)
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Không thể đọc cấu hình form", 400));
                }
                var data = await _formConfigRepository.SyncDataFromFormAdvancedAsync(request, formConfig);
                if (data.type == 0)
                {
                    return BadRequest(ServiceResponse<object>.CreateError(data.message, 400));
                }    
                var response = ServiceResponse<object>.CreateSuccess(data.formConfig, "Lấy dữ liệu từ chứng từ thành công");
                var json = JsonConvert.SerializeObject(response);
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gọi stored procedure.", detail = ex.Message });
            }
        }
        [HttpPost("GetFormData")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<object>>> GetFormData([FromBody] FormDataRequest request)
        {
            try
            {
                // Validate request
                if (request == null)
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Request không được để trống", 400));
                }

                if (string.IsNullOrEmpty(request.Controller))
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Controller không được để trống", 400));
                }

                if (string.IsNullOrEmpty(request.FormId))
                {
                    return BadRequest(ServiceResponse<object>.CreateError("FormId không được để trống", 400));
                }

                if (request.PrimaryKey == null || request.PrimaryKey.Length == 0)
                {
                    return BadRequest(ServiceResponse<object>.CreateError("PrimaryKey không được để trống", 400));
                }

                if (request.Value == null || request.Value.Length == 0)
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Value không được để trống", 400));
                }

                if (request.PrimaryKey.Length != request.Value.Length)
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Số lượng PrimaryKey và Value phải bằng nhau", 400));
                }

                if (request.Type == "voucher" && (string.IsNullOrEmpty(request.IdVC) || string.IsNullOrEmpty(request.VCDate)))
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Chứng từ, không được để trống IdVC hoặc VCDate", 400));
                }


                // Đọc form config từ file JSON
                var configContent = await _formConfigRepository.GetFormConfigAsync(request.Controller);
                var formConfig = JsonConvert.DeserializeObject<dynamic>(configContent);

                if (formConfig == null)
                {
                    return BadRequest(ServiceResponse<object>.CreateError("Không thể đọc cấu hình form", 400));
                }

                // Lấy dữ liệu từ database
                var sqlData = await _formConfigRepository.GetFormDataAsync(
                    request.FormId,
                    request.PrimaryKey,
                    request.Value,
                    request.Unit ?? "",
                    request.UserId ?? "",
                    request.Type,
                    request.VCDate ?? "");

                // Merge dữ liệu vào initialData của form config, chỉ lấy các key có trong fields
                if (formConfig.tabs != null && formConfig.tabs.Count > 0)
                {
                    foreach (var tab in formConfig.tabs)
                    {
                        if (tab.form != null && tab.form.fields != null)
                        {
                            var initialData = new Newtonsoft.Json.Linq.JObject();
                            foreach (var field in tab.form.fields)
                            {
                                string key = field.key;
                                if (sqlData.ContainsKey(key))
                                {
                                    initialData[key] = sqlData[key] == null ? null : Newtonsoft.Json.Linq.JToken.FromObject(sqlData[key]);
                                }
                            }
                            tab.form.initialData = initialData;
                        }

                        // Xử lý detail data - hỗ trợ nhiều detail con
                        if (tab.detail != null)
                        {
                            // Kiểm tra xem detail có phải là mảng không
                            if (tab.detail is Newtonsoft.Json.Linq.JArray detailArray)
                            {
                                // Xử lý từng detail trong mảng
                                for (int detailIndex = 0; detailIndex < detailArray.Count; detailIndex++)
                                {
                                    var detail = detailArray[detailIndex];
                                    if (detail != null && !string.IsNullOrEmpty(detail["formId"]?.ToString()))
                                    {
                                        try
                                        {
                                            string detailTableName = detail["formId"].ToString();
                                            string foreignKeyStr = detail["foreignKey"]?.ToString() ?? "";
                                            
                                            if (!string.IsNullOrEmpty(foreignKeyStr))
                                            {
                                                // Tách foreignKey thành mảng
                                                var foreignKeys = foreignKeyStr.Split(',').Select(fk => fk.Trim()).ToArray();
                                                
                                                // Tìm foreignKey trùng với primaryKey của master
                                                var matchingForeignKeys = new List<string>();
                                                var matchingValues = new List<string>();
                                                
                                                foreach (var fk in foreignKeys)
                                                {
                                                    if (request.PrimaryKey.Contains(fk))
                                                    {
                                                        int index = Array.IndexOf(request.PrimaryKey, fk);
                                                        if (index >= 0 && index < request.Value.Length)
                                                        {
                                                            matchingForeignKeys.Add(fk);
                                                            matchingValues.Add(request.Value[index]);
                                                        }
                                                    }
                                                }

                                                // Chỉ lấy dữ liệu detail nếu có foreignKey trùng với primaryKey
                                                if (matchingForeignKeys.Count > 0)
                                                {
                                                    // Lấy dữ liệu detail từ database
                                                    var detailData = await _formConfigRepository.GetDetailDataAsync(
                                                        detailTableName,
                                                        matchingForeignKeys.ToArray(),
                                                        matchingValues.ToArray(),
                                                        request.Unit ?? "",
                                                        request.UserId ?? "",
                                                        request.Type,
                                                        request.VCDate ?? "");

                                                    // Chuyển đổi thành JArray để gán vào initialData
                                                    var dataArray = new Newtonsoft.Json.Linq.JArray();
                                                    foreach (var detailRow in detailData)
                                                    {
                                                        var detailObj = new Newtonsoft.Json.Linq.JObject();
                                                        foreach (var kvp in detailRow)
                                                        {
                                                            detailObj[kvp.Key] = kvp.Value == null ? null : Newtonsoft.Json.Linq.JToken.FromObject(kvp.Value);
                                                        }
                                                        dataArray.Add(detailObj);
                                                    }

                                                    detail["initialData"] = dataArray;
                                                }
                                                else
                                                {
                                                    // Không có foreignKey trùng, để initialData rỗng
                                                    detail["initialData"] = new Newtonsoft.Json.Linq.JArray();
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            string tabTitle = tab["title"] != null ? tab["title"].ToString() : "Unknown";
                                            string detailTitle = detail["title"] != null ? detail["title"].ToString() : "Unknown";
                                            _logger.LogWarning(ex, "Lỗi khi xử lý detail data cho tab {TabTitle}, detail {DetailTitle}", tabTitle, detailTitle);
                                            // Không throw exception để không ảnh hưởng đến việc lấy dữ liệu master
                                            detail["initialData"] = new Newtonsoft.Json.Linq.JArray();
                                        }
                                    }
                                }
                            }
                            else if (tab.detail is Newtonsoft.Json.Linq.JObject detailObj)
                            {
                                // Xử lý detail đơn lẻ (backward compatibility)
                                if (!string.IsNullOrEmpty(detailObj["entity"]?.ToString()))
                                {
                                    try
                                    {
                                        string detailTableName = detailObj["entity"].ToString();
                                        string foreignKeyStr = detailObj["foreignKey"]?.ToString() ?? "";
                                        
                                        if (!string.IsNullOrEmpty(foreignKeyStr))
                                        {
                                            // Tách foreignKey thành mảng
                                            var foreignKeys = foreignKeyStr.Split(',').Select(fk => fk.Trim()).ToArray();
                                            
                                            // Tìm foreignKey trùng với primaryKey của master
                                            var matchingForeignKeys = new List<string>();
                                            var matchingValues = new List<string>();
                                            
                                            foreach (var fk in foreignKeys)
                                            {
                                                if (request.PrimaryKey.Contains(fk))
                                                {
                                                    int index = Array.IndexOf(request.PrimaryKey, fk);
                                                    if (index >= 0 && index < request.Value.Length)
                                                    {
                                                        matchingForeignKeys.Add(fk);
                                                        matchingValues.Add(request.Value[index]);
                                                    }
                                                }
                                            }

                                            // Chỉ lấy dữ liệu detail nếu có foreignKey trùng với primaryKey
                                            if (matchingForeignKeys.Count > 0)
                                            {
                                                // Lấy dữ liệu detail từ database
                                                var detailData = await _formConfigRepository.GetDetailDataAsync(
                                                    detailTableName,
                                                    matchingForeignKeys.ToArray(),
                                                    matchingValues.ToArray(),
                                                    request.Unit ?? "",
                                                    request.UserId ?? "",
                                                    request.Type,
                                                    request.VCDate ?? "");

                                                // Chuyển đổi thành JArray để gán vào initialData
                                                var dataArray = new Newtonsoft.Json.Linq.JArray();
                                                foreach (var detailRow in detailData)
                                                {
                                                    var rowObj = new Newtonsoft.Json.Linq.JObject();
                                                    foreach (var kvp in detailRow)
                                                    {
                                                        rowObj[kvp.Key] = kvp.Value == null ? null : Newtonsoft.Json.Linq.JToken.FromObject(kvp.Value);
                                                    }
                                                    dataArray.Add(rowObj);
                                                }

                                                detailObj["initialData"] = dataArray;
                                            }
                                            else
                                            {
                                                // Không có foreignKey trùng, để initialData rỗng
                                                detailObj["initialData"] = new Newtonsoft.Json.Linq.JArray();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string tabTitle = tab["title"] != null ? tab["title"].ToString() : "Unknown";
                                        _logger.LogWarning(ex, "Lỗi khi xử lý detail data cho tab {TabTitle}", tabTitle);
                                        // Không throw exception để không ảnh hưởng đến việc lấy dữ liệu master
                                        detailObj["initialData"] = new Newtonsoft.Json.Linq.JArray();
                                    }
                                }
                            }
                        }
                    }
                }

                // Thêm IdVC và VCDate vào response nếu type là voucher
                if (request.Type == "voucher")
                {
                    formConfig.idVC = request.IdVC ?? "";
                    formConfig.VCDate = request.VCDate ?? "";
                }

                var response = ServiceResponse<object>.CreateSuccess(formConfig, "Lấy dữ liệu form thành công");
                var json = JsonConvert.SerializeObject(response);
                return Content(json, "application/json");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(ServiceResponse<object>.CreateError(ex.Message, 400));
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Form configuration file not found");
                return NotFound(ServiceResponse<object>.CreateError(ex.Message, 404));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error processing JSON");
                return StatusCode(500, ServiceResponse<object>.CreateError("Lỗi xử lý dữ liệu JSON", 500));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting form data");
                return StatusCode(500, ServiceResponse<object>.CreateError("Đã xảy ra lỗi không mong muốn", 500));
            }
        }
    }
} 