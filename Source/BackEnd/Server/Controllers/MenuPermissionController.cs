using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using reportSystem01.Server.Data;
using reportSystem01.Shared;
using Sinco.Server.Repositories.Permission;
using System.Linq;

namespace reportSystem01.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuPermissionController : ControllerBase
    {
        private readonly IMenuPermissionService _menuPermissionService;

        public MenuPermissionController(IMenuPermissionService menuPermissionService)
        {
            _menuPermissionService = menuPermissionService;
        }

        [HttpGet("user/{userId}")]
        //[Authorize]
        public async Task<ActionResult<HashSet<MenuPermissionDTO>>> GetUserPermissions(int userId)
        {
            var permissions = await _menuPermissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        [HttpPost("SaveUserPermissions")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<string>>> SaveUserPermissions([FromBody] SavePermissionRequestDTO request)
        {
            try
            {
                if (request == null || request.UserId <= 0 || request.Permissions == null)
                {
                    return BadRequest(new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                await _menuPermissionService.SaveUserPermissionsAsync(request.UserId, new HashSet<MenuPermissionDTO>(request.Permissions));

                return Ok(new ServiceResponse<string>
                {
                    Success = true,
                    Message = "Permissions saved successfully"
                });
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return StatusCode(500, new ServiceResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while saving permissions"
                });
            }
        }

        [HttpGet("usergroup/{groupId}")]
        //[Authorize]
        public async Task<ActionResult<HashSet<MenuPermissionDTO>>> GetUserGroupPermissions(int groupId)
        {
            try
            {
                var permissions = await _menuPermissionService.GetUserGroupPermissionsAsync(groupId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("SaveUserGroupPermissions")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<string>>> SaveUserGroupPermissions([FromBody] SaveGroupPermissionRequestDTO request)
        {
            try
            {
                if (request == null || request.UserGroupId <= 0 || request.Permissions == null)
                {
                    return BadRequest(new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "Dữ liệu yêu cầu không hợp lệ"
                    });
                }

                await _menuPermissionService.SaveUserGroupPermissionsAsync(request.UserGroupId, new HashSet<MenuPermissionDTO>(request.Permissions));

                return Ok(new ServiceResponse<string>
                {
                    Success = true,
                    Message = "Lưu phân quyền thành công"
                });
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return StatusCode(500, new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi lưu phân quyền"
                });
            }
        }

        [HttpPost("SaveFlatUserPermissions")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<string>>> SaveFlatUserPermissions([FromBody] SaveFlatPermissionRequestDTO request)
        {
            try
            {
                if (request == null || request.UserId <= 0 || request.Permissions == null)
                {
                    return BadRequest(new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "Invalid request data"
                    });
                }

                Console.WriteLine($"Received request for user {request.UserId} with {request.Permissions.Count} permissions");
                foreach (var permission in request.Permissions)
                {
                    Console.WriteLine($"Permission: {permission.Id} - {permission.Name} - Access: {permission.HasAccess}");
                }

                await _menuPermissionService.SaveFlatUserPermissionsAsync(request.UserId, request.Permissions);

                return Ok(new ServiceResponse<string>
                {
                    Success = true,
                    Message = "Permissions saved successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving permissions: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"An error occurred while saving permissions: {ex.Message}"
                });
            }
        }
    }
}
