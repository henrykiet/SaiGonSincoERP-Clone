using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using reportSystem01.Shared;

namespace reportSystem01.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<UserDto> _userManager;

        public UserController(UserManager<UserDto> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<List<UserDto>>>> SearchUsers([FromQuery] string searchText)
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.UserName.Contains(searchText) || 
                               u.Email.Contains(searchText))
                    .Take(10)
                    .Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        Email = u.Email
                    })
                    .ToListAsync();

                return Ok(new ServiceResponse<List<UserDto>>
                {
                    Data = users,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return Ok(new ServiceResponse<List<UserDto>>
                {
                    Success = false,
                    Message = $"Error searching users: {ex.Message}"
                });
            }
        }
    }
} 