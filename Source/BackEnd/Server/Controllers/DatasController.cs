using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using reportSystem01.Shared;
using Sinco.Server.Repositories;
using Sinco.Server.SqlJsonDefinations.Requests;

namespace Sinco.Server.Controllers
{
	[Route("api/data")]
	[ApiController]
	public class DatasController : ControllerBase
	{
		private readonly IDataService _dataService;
		public DatasController(IDataService dataService)
		{
			_dataService = dataService;
		}
		[HttpGet("metadata/{controll}")]
		public async Task<IActionResult> GetMetadata([FromRoute]string controll , [FromQuery] MetadataRequest request)//controller là đường dẫn
		{
			if ( request == null )
			{
				return BadRequest(new ServiceResponse<dynamic>
				{
					Success = false ,
					Message = $"Missing required parameters in request" ,
					Data = default
				});
			}
			var result = await _dataService.GetMetadata(controll , request);
			return Ok(result);
		}
		[HttpGet("list")]
		public async Task<IActionResult> GetDataList([FromQuery] DataListRequest request)
		{
			var result = await _dataService.GetDataList(request);
			return Ok(result);
		}
		[HttpPost("save")]
		public async Task<IActionResult> PostDataSave([FromBody] DataSaveRequest request)
		{
			var result = await _dataService.PostDataSave(request);
			return Ok(result);
		}
		[HttpDelete("delete")]
		public async Task<IActionResult> DeleteData([FromBody] DataDeleteRequest request)
		{
			var result = await _dataService.DeleteData(request);
			return Ok(result);
		}
		[HttpDelete("deletedMulti")]
		public async Task<IActionResult> deletedMultiData([FromBody] DataDeleteRequest request)
		{
			if(request == null || string.IsNullOrEmpty(request.FormId) || string.IsNullOrEmpty(request.PrimaryKey) || request.Ids == null || request.Ids.Count == 0 || string.IsNullOrEmpty(request.Status))
			{
				return BadRequest(new ServiceResponse<dynamic>
				{
					Success = false ,
					Message = $"Missing required parameters in request" ,
					Data = default
				});
            }
            var result = await _dataService.DeletedMultiData(request);
			return Ok(result);
		}
		[HttpGet("options/{optionId}")]
		public async Task<IActionResult> GetDataOptions([FromRoute] string optionId , [FromQuery] DataOptionsRequest request)
		{
			var result = await _dataService.GetDataOptions(optionId , request);
			return Ok(result);
		}
		[HttpGet("lookup/{formId}/{invoice}")]
		public async Task<IActionResult> GetDataLookup([FromRoute] string formId , [FromRoute] string invoice , [FromQuery] DataLookupRequest request)
		{
			var result = await _dataService.GetDataLookup(formId , request);
			return Ok(result);
		}
		[HttpGet("report/{formId}")]
		public async Task<IActionResult> GetDataReport([FromRoute] string formId , [FromQuery] DataReportRequest request)
		{
			var result = await _dataService.GetDataReport(formId , request);
			return Ok(result);
		}
		[HttpGet("report/pivot/{formId}")]
		public async Task<IActionResult> GetDataReportPivot([FromRoute] string formId , [FromQuery] DataReportPivotRequest request)
		{
			var result = await _dataService.GetDataReportPivot(formId , request);
			return Ok(result);
		}
		[HttpPost("custom")]
		public async Task<IActionResult> PostDataCustom([FromBody] DataCustomRequest request)
		{
			var result = await _dataService.PostDataCustom(request);
			return Ok(result);
		}
	}
}
