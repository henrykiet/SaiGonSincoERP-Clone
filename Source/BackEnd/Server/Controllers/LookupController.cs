using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using reportSystem01.Shared;
using Sinco.Server.Repositories.Lookup;

namespace Sinco.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : ControllerBase
    {
        private readonly ILookupRepository _lookupRepository;

        public LookupController(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository ?? throw new ArgumentNullException(nameof(lookupRepository));
        }

        [HttpPost]
        public async Task<IActionResult> LookupAsync([FromBody] JsonElement request)
        {
            try
            {
                // Validation đầu vào
                if (!request.TryGetProperty("controller", out var controllerProp))
                    return BadRequest(ServiceResponse<object>.CreateError("Thiếu trường 'controller'"));

                var controller = controllerProp.GetString();
                if (string.IsNullOrWhiteSpace(controller))
                    return BadRequest(ServiceResponse<object>.CreateError("Trường 'controller' không được để trống"));

                // Đọc language, unit, userId (optional)
                var language = "";
                var unit = "";
                var userId = "";

                if (request.TryGetProperty("language", out var languageProp))
                    language = languageProp.GetString() ?? "";
                
                if (request.TryGetProperty("unit", out var unitProp))
                    unit = unitProp.GetString() ?? "";
                
                if (request.TryGetProperty("userId", out var userIdProp))
                    userId = userIdProp.GetString() ?? "";

                // Đọc filters (optional)
                var filters = new List<FilterItem>();
                if (request.TryGetProperty("filter", out var filterProp) && filterProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in filterProp.EnumerateArray())
                    {
                        try
                        {
                            var field = item.GetProperty("field").GetString();
                            var value = item.GetProperty("value").GetString();
                            var op = item.GetProperty("operator").GetString();

                            if (!string.IsNullOrEmpty(field) && !string.IsNullOrEmpty(op))
                            {
                                filters.Add(new FilterItem
                                {
                                    Field = field,
                                    Value = value ?? "",
                                    Operator = op
                                });
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                            return BadRequest(ServiceResponse<object>.CreateError("Filter item thiếu field bắt buộc (field, operator)"));
                        }
                    }
                }

                // Gọi repository với thông tin bổ sung
                var result = await _lookupRepository.LookupAsync(controller, filters, language, unit, userId);
                return Ok(ServiceResponse<object>.CreateSuccess(result, "Thành công"));
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ServiceResponse<object>.CreateError(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ServiceResponse<object>.CreateError(ex.Message));
            }
            catch (Exception ex)
            {
                // Log exception here
                return StatusCode(500, ServiceResponse<object>.CreateError("Lỗi hệ thống"));
            }
        }

        public class FilterItem
        {
            public string Field { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Operator { get; set; } = string.Empty;
        }
    }
}