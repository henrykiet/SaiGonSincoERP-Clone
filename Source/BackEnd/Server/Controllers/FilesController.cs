using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sinco.Server.Repositories;
using System.IO;

namespace Sinco.Server.Controllers
{
	[Route("api/AttachedFile")]
	[ApiController]
	public class FilesController : ControllerBase
	{
		private readonly IFileService _fileService;
		public FilesController(IFileService fileService)
		{
			_fileService = fileService;
		}

		[HttpPost("import-file")]
		public async Task<IActionResult> ImportFile([FromForm] FileRequest fileRequest)
		{
			if ( fileRequest.Type == "import" && fileRequest.File == null )
			{
				return BadRequest(new
				{
					message = "File is required." ,
				});
			}

			if ( string.IsNullOrEmpty(fileRequest.Controll) )
			{
				return BadRequest(new
				{
					message = "Controller name is required." ,
				});
			}

			var result = await _fileService.ImportFileAsync(fileRequest);

			// Nếu là trả về file mẫu thì trả ra dạng file
			if ( fileRequest.Type == "template" )
			{
				if ( !result.Success || result.Data is not ImportFileResponse fileResult )
				{
					return BadRequest(new
					{
						message = result.Message ?? "Tạo file mẫu thất bại." ,
						data = result.Data
					});
				}

				return File(fileResult.FileBytes , fileResult.ContentType , fileResult.FileName);
			}

			// Còn lại là kết quả xử lý import (hoặc lỗi định dạng)

			return StatusCode(result.StatusCode, new
			{
				succes = result.Success,
				message = result.Message,
				data = result.Data
			});
		}


		[HttpPost("export")]
		public async Task<IActionResult> ExportPdf([FromBody] ReportRequest request)
		{
			if ( string.IsNullOrEmpty(request.Controll) )
			{
				return BadRequest("Table name is required");
			}

			var result = await _fileService.ExportPdfAsync(request);
			if ( result.Success == true && result.Data != null )
			{
				var contentType = result.Message.Contains(".pdf") ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				var fileName = result.Message;
                HttpContext.Response.Headers["X-Debug-Filename"] = fileName;
                HttpContext.Response.Headers["X-Debug-Content-Type"] = contentType;
                return File(result.Data , contentType , fileName);
			}
			else
			{
				return BadRequest(result);
			}
		}
	}
}
