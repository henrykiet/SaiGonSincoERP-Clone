using Microsoft.EntityFrameworkCore;
using reportSystem01.Server.Data;
using reportSystem01.Server.Helpers;
using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Permission
{
    public class MenuService : IMenuService
    {
        private readonly ReportServerContext _context;

        public MenuService(ReportServerContext context)
        {
            _context = context;
        }

        public async Task<List<MenuDto>> GetUserMenuPermissionsAsync(int userId)
        {
            var role = _context.Users.Where(u => u.UserId == userId).Select(u => u.Role).FirstOrDefault();

            if (role == "Admin")
            {
                return await (from m in _context.Menus
                              select new MenuDto
                              {
                                  MenuId = m.MenuId,
                                  Name = m.MenuName,
                                  Name2 = m.MenuName2,
                                  ParentMenuId = m.ParentMenuId,
                                  Icon = m.Icon,
                                  Url = m.SysId,
                                  VoucherCode = m.VoucherCode,
                                  Type = m.TypeMenu,
                                  HasAccess = true,
                                  HasInsert = true,
                                  HasUpdate = true,
                                  HasDel = true
                              }).ToListAsync();
            }
            else
            {
                return await (from m in _context.Menus
                              join p in _context.UserMenuPermissions on m.MenuId equals p.MenuId
                              where p.UserId == userId && p.RAccess == 1
                              select new MenuDto
                              {
                                  MenuId = m.MenuId,
                                  Name = m.MenuName,
                                  Name2 = m.MenuName2,
                                  ParentMenuId = m.ParentMenuId,
                                  Icon = m.Icon,
                                  Url = m.SysId,
                                  VoucherCode = m.VoucherCode,
                                  Type = m.TypeMenu,
                                  HasAccess = true,
                                  HasInsert = (p.RInsert == 1 ? true : false),
                                  HasUpdate = (p.RUpdate == 1 ? true : false),
                                  HasDel = (p.RDel == 1 ? true : false)
                              }).ToListAsync();
            }
        }


        public async Task<List<MenuDto>> GetGroupMenuPermissionsAsync(int userId)
        {
            var role = _context.Users.Where(u => u.UserId == userId).Select(u => u.Role).FirstOrDefault();
            if (role == "Admin")
            {
                return await (from m in _context.Menus
                              select new MenuDto
                              {
                                  MenuId = m.MenuId,
                                  Name = m.MenuName,
                                  Name2 = m.MenuName2,
                                  ParentMenuId = m.ParentMenuId,
                                  Icon = m.Icon,
                                  Url = m.SysId,
                                  VoucherCode = m.VoucherCode,
                                  Type = m.TypeMenu,
                                  HasAccess = true,
                                  HasInsert = true,
                                  HasUpdate = true,
                                  HasDel = true,
                              }).ToListAsync();
            }
            else
            {
                return await (from m in _context.Menus
                              join gp in _context.UserGroupMenuPermissions on m.MenuId equals gp.MenuId
                              join ug in _context.UserGroups on gp.UserGroupId equals ug.UserGroupId
                              where ug.ListUser.Contains(userId.ToString()) && gp.RAccess == 1
                              select new MenuDto
                              {
                                  MenuId = m.MenuId,
                                  Name = m.MenuName,
                                  Name2 = m.MenuName2,
                                  ParentMenuId = m.ParentMenuId,
                                  Icon = m.Icon,
                                  Url = m.SysId,
                                  VoucherCode = m.VoucherCode,
                                  Type = m.TypeMenu,
                                  HasAccess = true,
                                  HasInsert = (gp.RInsert == 1 ? true : false),
                                  HasUpdate = (gp.RUpdate == 1 ? true : false),
                                  HasDel = (gp.RDel == 1 ? true : false)
                              }).ToListAsync();
            }
        }

        public async Task<List<MenuDto>> GetAllMenu()
        {
            return await (from m in _context.Menus
                          select new MenuDto
                          {
                              MenuId = m.MenuId,
                              Name = m.MenuName,
                              Name2 = m.MenuName2,
                              ParentMenuId = m.ParentMenuId,
                              Icon = m.Icon,
                              Url = m.SysId,
                              VoucherCode = m.VoucherCode,
                              Type = m.TypeMenu,
                              HasAccess = true,
                              HasInsert = true,
                              HasUpdate = true,
                              HasDel = true
                          }).ToListAsync();
        }
    }
}
