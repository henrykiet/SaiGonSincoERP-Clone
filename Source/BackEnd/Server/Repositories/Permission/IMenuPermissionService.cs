using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Permission
{
    public interface IMenuPermissionService
    {
        Task<HashSet<MenuPermissionDTO>> GetUserPermissionsAsync(int userId);
        Task SaveUserPermissionsAsync(int userId, HashSet<MenuPermissionDTO> permissions);

        // New group permissions methods
        Task<HashSet<MenuPermissionDTO>> GetUserGroupPermissionsAsync(int groupId);
        Task SaveUserGroupPermissionsAsync(int groupId, HashSet<MenuPermissionDTO> permissions);
        Task SaveFlatUserPermissionsAsync(int userId, List<FlatMenuPermissionDTO> permissions);
    }
}
