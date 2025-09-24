using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using reportSystem01.Shared;
using Sinco.Server.Repositories.Auth;

namespace reportSystem01.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly IUserGroupService _userGroupService;
        public UserGroupController(IUserGroupService userGroupService)
        {
            _userGroupService = userGroupService;
        }

        [HttpGet("getAllUserGroups")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<List<UserGroupDto>>>> GetAllUserGroups()
        {
            var response = await _userGroupService.GetAllUserGroups();
            return Ok(response);
        }

        [HttpGet("{id}")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<UserGroupDto>>> GetUserGroupById(int id)
        {
            var response = await _userGroupService.GetUserGroupById(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpPost("createUserGroup")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<int>>> CreateUserGroup(UserGroupDto request)
        {
            var response = await _userGroupService.CreateUserGroup(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("updateUserGroup")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<UserGroupDto>>> UpdateUserGroup(UserGroupDto request)
        {
            var response = await _userGroupService.UpdateUserGroup(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("deleteUserGroup/{id}")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteUserGroup(int id)
        {
            var response = await _userGroupService.DeleteUserGroup(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("updateTreeViewPermissions")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> UpdateTreeViewPermissions(TreeViewPermissionDto request)
        {
            var response = await _userGroupService.UpdateTreeViewPermissions(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
