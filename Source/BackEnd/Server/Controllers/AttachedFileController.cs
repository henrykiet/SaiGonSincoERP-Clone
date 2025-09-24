using Microsoft.AspNetCore.Mvc;
using Sinco.Server.Models;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using reportSystem01.Shared;
using Sinco.Server.Repositories.AttachedFile;

namespace Sinco.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý file đính kèm")]
    public class AttachedFileController : ControllerBase
    {
        private readonly IAttachedFileRepository _fileRepository;

        public AttachedFileController(IAttachedFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        [HttpPost]
        [SwaggerOperation(
    Summary = "Tải file lên",
    Description = "Tải một file lên hệ thống",
    OperationId = "UploadFile",
    Tags = new[] { "File" }
)]
        [SwaggerResponse(200, "Tải file lên thành công")]
        [SwaggerResponse(400, "Lỗi khi tải file")]
        public async Task<ActionResult<ServiceResponse<bool>>> UploadFile([FromForm] IFormFile file, [FromForm] string controllerName, [FromForm] string sysKey)
        {
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(sysKey))
                return BadRequest(ServiceResponse<bool>.CreateError("Controller và SysKey không được để trống", 400));

            if (file == null || file.Length == 0)
                return BadRequest(ServiceResponse<bool>.CreateError("Không có file được tải lên", 400));

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var fileContent = new byte[file.Length];
                    await stream.ReadAsync(fileContent, 0, (int)file.Length);

                    var fileAttachment = new FileAttachment
                    {
                        Controller = controllerName,
                        SysKey = sysKey,
                        FileName = file.FileName,
                        FileContent = fileContent,
                        ContentType = file.ContentType
                    };

                    var result = await _fileRepository.UploadFileAsync(fileAttachment);
                    if (result)
                        return Ok(ServiceResponse<bool>.CreateSuccess(true, "Tải file lên thành công"));
                    else
                        return BadRequest(ServiceResponse<bool>.CreateError("Không thể tải file lên", 400));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServiceResponse<bool>.CreateError($"Lỗi khi xử lý file: {ex.Message}", 500));
            }
        }

        [HttpGet("{controllerName}/{sysKey}")]
        [SwaggerOperation(
            Summary = "Lấy danh sách file",
            Description = "Lấy danh sách các file đính kèm theo controller và sysKey",
            OperationId = "GetFiles",
            Tags = new[] { "File" }
        )]
        [SwaggerResponse(200, "Lấy danh sách file thành công")]
        public async Task<ActionResult<ServiceResponse<IEnumerable<FileAttachment>>>> GetFiles(string controllerName, string sysKey)
        {
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(sysKey))
                return BadRequest(ServiceResponse<IEnumerable<FileAttachment>>.CreateError("Controller và SysKey không được để trống", 400));

            try
            {
                var files = await _fileRepository.GetFilesAsync(controllerName, sysKey);
                return Ok(ServiceResponse<IEnumerable<FileAttachment>>.CreateSuccess(files));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServiceResponse<IEnumerable<FileAttachment>>.CreateError($"Lỗi khi lấy danh sách file: {ex.Message}", 500));
            }
        }

        [HttpGet("{controllerName}/{sysKey}/{fileName}")]
        [SwaggerOperation(
            Summary = "Tải file",
            Description = "Tải một file cụ thể theo tên",
            OperationId = "GetFile",
            Tags = new[] { "File" }
        )]
        [SwaggerResponse(200, "Tải file thành công")]
        [SwaggerResponse(404, "Không tìm thấy file")]
        public async Task<ActionResult<ServiceResponse<FileAttachment>>> GetFile(string controllerName, string sysKey, string fileName)
        {
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(sysKey) || string.IsNullOrEmpty(fileName))
                return BadRequest(ServiceResponse<FileAttachment>.CreateError("Controller, SysKey và FileName không được để trống", 400));

            try
            {
                var file = await _fileRepository.GetFileAsync(controllerName, sysKey, fileName);
                if (file == null)
                    return NotFound(ServiceResponse<FileAttachment>.CreateError("Không tìm thấy file", 404));

                return Ok(ServiceResponse<FileAttachment>.CreateSuccess(file));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServiceResponse<FileAttachment>.CreateError($"Lỗi khi tải file: {ex.Message}", 500));
            }
        }

        [HttpDelete("{controllerName}/{sysKey}/{fileName}")]
        [SwaggerOperation(
            Summary = "Xóa file",
            Description = "Xóa một file theo tên",
            OperationId = "DeleteFile",
            Tags = new[] { "File" }
        )]
        [SwaggerResponse(200, "Xóa file thành công")]
        [SwaggerResponse(404, "Không tìm thấy file")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteFile(string controllerName, string sysKey, string fileName)
        {
            if (string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(sysKey) || string.IsNullOrEmpty(fileName))
                return BadRequest(ServiceResponse<bool>.CreateError("Controller, SysKey và FileName không được để trống", 400));

            try
            {
                var result = await _fileRepository.DeleteFileAsync(controllerName, sysKey, fileName);
                if (!result)
                    return NotFound(ServiceResponse<bool>.CreateError("Không tìm thấy file", 404));

                return Ok(ServiceResponse<bool>.CreateSuccess(true, "Xóa file thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ServiceResponse<bool>.CreateError($"Lỗi khi xóa file: {ex.Message}", 500));
            }
        }
    }
} 