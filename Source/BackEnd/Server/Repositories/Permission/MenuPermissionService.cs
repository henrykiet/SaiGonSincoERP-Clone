using Microsoft.EntityFrameworkCore;
using reportSystem01.Server.Data;
using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Permission
{
    public class MenuPermissionService : IMenuPermissionService
    {
        private readonly ReportServerContext _context;

        public MenuPermissionService(ReportServerContext context)
        {
            _context = context;
        }

        public async Task<HashSet<MenuPermissionDTO>> GetUserPermissionsAsync(int userId)
        {
            // Lấy tất cả menu
            var allMenus = await _context.Menus
                .OrderBy(m => m.MenuId)
                .ToListAsync();

            // Lấy permissions của user
            var userPermissions = await _context.UserMenuPermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // Xây dựng cây menu từ danh sách phẳng
            var menuTree = BuildMenuTreeFromFlatList(allMenus, userPermissions);
            return new HashSet<MenuPermissionDTO>(menuTree);
        }

        private List<MenuPermissionDTO> BuildMenuTreeFromFlatList(List<Menu> allMenus, List<UserMenuPermission> permissions)
        {
            var lookup = allMenus.ToLookup(x => x.ParentMenuId);
            return BuildMenuTreeRecursive(lookup, "", permissions);
        }

        private List<MenuPermissionDTO> BuildMenuTreeRecursive(ILookup<string, Menu> lookup, string parentId, List<UserMenuPermission> permissions)
        {
            var result = new List<MenuPermissionDTO>();
            var children = lookup[parentId];

            foreach (var menu in children)
            {
                var permission = permissions.FirstOrDefault(p => p.MenuId == menu.MenuId);
                var dto = new MenuPermissionDTO
                {
                    Id = menu.MenuId,
                    Name = menu.MenuName,
                    ParentMenuId = menu.ParentMenuId ?? string.Empty,
                    HasAccess = permission?.RAccess == 1 ? true : false,
                    CanInsert = permission?.RInsert == 1 ? true : false,
                    CanUpdate = permission?.RUpdate == 1 ? true : false,
                    CanDelete = permission?.RDel == 1 ? true : false
                };

                // Đệ quy để lấy các menu con
                var childMenus = BuildMenuTreeRecursive(lookup, menu.MenuId, permissions);
                if (childMenus.Any())
                {
                    dto.Children = new HashSet<MenuPermissionDTO>(childMenus);
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task SaveUserPermissionsAsync(int userId, HashSet<MenuPermissionDTO> permissions)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa permissions cũ
                var existingPermissions = await _context.UserMenuPermissions
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
                _context.UserMenuPermissions.RemoveRange(existingPermissions);

                // Thêm permissions mới từ dữ liệu phẳng
                var newPermissions = permissions
                    .Select(p => new UserMenuPermission
                    {
                        UserId = userId,
                        MenuId = p.Id,
                        RAccess = p.HasAccess ? 1 : 0,
                        RInsert = p.CanInsert ? 1 : 0,
                        RUpdate = p.CanUpdate ? 1 : 0,
                        RDel = p.CanDelete ? 1 : 0
                    });

                await _context.UserMenuPermissions.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private IEnumerable<MenuPermissionDTO> FlattenPermissions(IEnumerable<MenuPermissionDTO> permissions)
        {
            foreach (var permission in permissions)
            {
                yield return permission;
                if (permission.Children != null)
                {
                    foreach (var child in FlattenPermissions(permission.Children))
                    {
                        yield return child;
                    }
                }
            }
        }

        public async Task<HashSet<MenuPermissionDTO>> GetUserGroupPermissionsAsync(int groupId)
        {
            // Lấy tất cả menu
            var allMenus = await _context.Menus
                .OrderBy(m => m.MenuId)
                .ToListAsync();

            // Lấy permissions của group
            var groupPermissions = await _context.UserGroupMenuPermissions
                .Where(p => p.UserGroupId == groupId)
                .ToListAsync();

            // Xây dựng cây menu từ danh sách phẳng
            var menuTree = BuildMenuTreeForGroupFromFlatList(allMenus, groupPermissions);
            return new HashSet<MenuPermissionDTO>(menuTree);
        }

        private List<MenuPermissionDTO> BuildMenuTreeForGroupFromFlatList(List<Menu> allMenus, List<UserGroupMenuPermission> permissions)
        {
            var lookup = allMenus.ToLookup(x => x.ParentMenuId);
            return BuildMenuTreeForGroupRecursive(lookup, "", permissions);
        }

        private List<MenuPermissionDTO> BuildMenuTreeForGroupRecursive(ILookup<string, Menu> lookup, string parentId, List<UserGroupMenuPermission> permissions)
        {
            var result = new List<MenuPermissionDTO>();
            var children = lookup[parentId];

            foreach (var menu in children)
            {
                var permission = permissions.FirstOrDefault(p => p.MenuId == menu.MenuId);
                var dto = new MenuPermissionDTO
                {
                    Id = menu.MenuId,
                    Name = menu.MenuName,
                    ParentMenuId = menu.ParentMenuId ?? string.Empty,
                    HasAccess = permission?.RAccess == 1,
                    CanInsert = permission?.RInsert == 1,
                    CanUpdate = permission?.RUpdate == 1,
                    CanDelete = permission?.RDel == 1
                };

                // Đệ quy để lấy các menu con
                var childMenus = BuildMenuTreeForGroupRecursive(lookup, menu.MenuId, permissions);
                if (childMenus.Any())
                {
                    dto.Children = new HashSet<MenuPermissionDTO>(childMenus);
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task SaveUserGroupPermissionsAsync(int groupId, HashSet<MenuPermissionDTO> permissions)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa permissions cũ của group
                var existingPermissions = await _context.UserGroupMenuPermissions
                    .Where(p => p.UserGroupId == groupId)
                    .ToListAsync();
                _context.UserGroupMenuPermissions.RemoveRange(existingPermissions);

                // Thêm permissions mới cho group
                var newPermissions = FlattenPermissions(permissions)
                    .Select(p => new UserGroupMenuPermission
                    {
                        UserGroupId = groupId,
                        MenuId = p.Id,
                        RAccess = p.HasAccess ? 1 : 0,
                        RInsert = p.CanInsert ? 1 : 0,
                        RUpdate = p.CanUpdate ? 1 : 0,
                        RDel = p.CanDelete ? 1 : 0
                    });

                await _context.UserGroupMenuPermissions.AddRangeAsync(newPermissions);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task SaveFlatUserPermissionsAsync(int userId, List<FlatMenuPermissionDTO> permissions)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"Starting save permissions for user {userId}");

                // Xóa permissions cũ
                var existingPermissions = await _context.UserMenuPermissions
                    .Where(p => p.UserId == userId)
                    .ToListAsync();
                Console.WriteLine($"Found {existingPermissions.Count} existing permissions to delete");
                _context.UserMenuPermissions.RemoveRange(existingPermissions);

                // Thêm permissions mới từ dữ liệu phẳng
                var newPermissions = permissions
                    .Select(p => new UserMenuPermission
                    {
                        UserId = userId,
                        MenuId = p.Id,
                        RAccess = p.HasAccess ? 1 : 0,
                        RInsert = p.CanInsert ? 1 : 0,
                        RUpdate = p.CanUpdate ? 1 : 0,
                        RDel = p.CanDelete ? 1 : 0
                    }).ToList();

                Console.WriteLine($"Creating {newPermissions.Count} new permissions");
                foreach (var permission in newPermissions)
                {
                    Console.WriteLine($"New permission: {permission.MenuId} - Access: {permission.RAccess}");
                }

                await _context.UserMenuPermissions.AddRangeAsync(newPermissions);
                var saveResult = await _context.SaveChangesAsync();
                Console.WriteLine($"Saved {saveResult} changes to database");

                await transaction.CommitAsync();
                Console.WriteLine("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveFlatUserPermissionsAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
