using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using reportSystem01.Shared;
using Sinco.Server.Models;
using Sinco.Server.Repositories.Custom;

namespace Sinco.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomQueryController : ControllerBase
    {
        private readonly CustomQueryRepository _repo;
        public CustomQueryController(CustomQueryRepository repo)
        {
            _repo = repo;
        }

        [HttpPost("execute")]
        //[Authorize]
        public async Task<IActionResult> Execute([FromBody] CustomQueryRequest request)
        {
            try
            {
                var data = await _repo.ExecuteCustomQueryAsync(request);
                return Ok(ServiceResponse<object>.CreateSuccess(data, "Request complete"));
            }
            catch (Exception ex)
            {
                return BadRequest(ServiceResponse<object>.CreateError(ex.Message));
            }
        }
    }
}
