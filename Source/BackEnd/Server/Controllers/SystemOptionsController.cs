using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using reportSystem01.Shared;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

[Route("api/system-options")]
[ApiController]
[Authorize]
public class SystemOptionsController : ControllerBase
{
    private readonly SystemOptionsRepository _repository;

    public SystemOptionsController(SystemOptionsRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOptionsAsyncGroup([FromQuery] string group = "")
    {
        var options = await _repository.GetAllOptionsAsyncGroup(group);
        return Ok(options);
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetAllGroups()
    {
        var groups = await _repository.GetAllGroupsAsync();
        return Ok(groups);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOption([FromBody] SystemOption option)
    {
        await _repository.CreateOptionAsync(option);
        return Ok(new { message = "Thêm thành công" });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOptionByIdAsync(int id)
    {

        var options =  await _repository.GetByIdAsync(id);
        return Ok(options);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOption(int id, [FromBody] SystemOption option)
    {
        option.Id = id;
        await _repository.UpdateAsync(option);
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOption(int id)
    {
        await _repository.DeleteAsync(id);
        return Ok(new { message = "Xóa thành công" });
    }
}
