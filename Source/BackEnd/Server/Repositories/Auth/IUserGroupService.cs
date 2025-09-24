using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Auth
{
    public interface IUserGroupService
    {
        Task<ServiceResponse<int>> CreateUserGroup(UserGroupDto request);
        Task<ServiceResponse<UserGroupDto>> UpdateUserGroup(UserGroupDto request);
        Task<ServiceResponse<bool>> DeleteUserGroup(int id);
        Task<ServiceResponse<List<UserGroupDto>>> GetAllUserGroups();
        Task<ServiceResponse<UserGroupDto>> GetUserGroupById(int id);
        Task<ServiceResponse<bool>> UpdateTreeViewPermissions(TreeViewPermissionDto request);

        Task<bool> UserGroupExists(string GroupName);
    }
}
