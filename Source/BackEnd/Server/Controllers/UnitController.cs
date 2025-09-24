using Microsoft.AspNetCore.Mvc;
using Sinco.Server.Models;
using reportSystem01.Shared;
using Sinco.Server.Repositories.Custom;

namespace Sinco.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitController : ControllerBase
    {
        private readonly IUnitRepository _unitRepository;

        public UnitController(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUnits()
        {
            try
            {
                var response = await _unitRepository.GetAllUnitsAsync();
                
                if (response.Success)
                {
                    return Ok(new
                    {
                        success = response.Success,
                        data = response.Data,
                        message = response.Message
                    });
                }
                else
                {
                    return StatusCode(response.StatusCode, new
                    {
                        success = response.Success,
                        data = (object?)null,
                        message = response.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    data = (object?)null,
                    message = "Có lỗi xảy ra khi lấy danh sách đơn vị"
                });
            }
        }
    }
} 