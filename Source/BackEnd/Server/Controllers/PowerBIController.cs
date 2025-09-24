using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PowerBIController : ControllerBase
{
    private readonly PowerBIService _powerBIService;

    public PowerBIController(PowerBIService powerBIService)
    {
        _powerBIService = powerBIService;
    }

    [HttpGet("embed-token/{reportId}")]
    public async Task<IActionResult> GetEmbedToken(string reportId)
    {
        var token = await _powerBIService.GetReportEmbedTokenAsync(reportId);
        return Ok(new { Token = token.Token });
    }
}
