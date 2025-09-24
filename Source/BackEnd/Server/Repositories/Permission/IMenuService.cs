using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Permission
{
    public interface IMenuService
    {
        Task<List<MenuDto>> GetUserMenuPermissionsAsync(int userId);
        Task<List<MenuDto>> GetGroupMenuPermissionsAsync(int userId);
        Task<List<MenuDto>> GetAllMenu();
    }
}
