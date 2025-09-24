using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using reportSystem01.Server.Helpers;
using reportSystem01.Shared;
using Sinco.Server.Repositories.Permission;
using System.IdentityModel.Claims;

namespace reportSystem01.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("user-menu/{userId}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<List<MenuDto>>>> GetUserMenu(int userId)
        {
            try
            {
                // Lấy menu từ user permissions
                var userMenus = await _menuService.GetUserMenuPermissionsAsync(userId);

                // Lấy menu từ group permissions
                var groupMenus = await _menuService.GetGroupMenuPermissionsAsync(userId);

                // Kết hợp và loại bỏ trùng lặp
                var allMenus = userMenus.Union(groupMenus, new MenuDtoComparer()).ToList();

                return Ok(new ServiceResponse<List<MenuDto>>
                {
                    Data = BuildMenuTree(allMenus),
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<List<MenuDto>>
                {
                    Success = false,
                    Message = $"Internal error: {ex.Message}"
                });
            }
        }

        [HttpGet("getAll-menu")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<List<MenuDto>>>> GetAllMenu()
        {
            try
            {
                // Lấy menu từ Menu
                var userMenus = await _menuService.GetAllMenu();

                return Ok(new ServiceResponse<List<MenuDto>>
                {
                    Data = BuildMenuTree(userMenus),
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<List<MenuDto>>
                {
                    Success = false,
                    Message = $"Internal error: {ex.Message}"
                });
            }
        }

        private List<MenuDto> BuildMenuTree(List<MenuDto> menus)
        {
            var lookup = menus.ToLookup(x => x.ParentMenuId);
            var rootMenus = lookup[""].OrderBy(x => x.MenuId).ToList();

            void AddChildren(MenuDto parent)
            {
                parent.Children = lookup[parent.MenuId].OrderBy(x => x.MenuId).ToList();
                foreach (var child in parent.Children)
                {
                    AddChildren(child);
                }
            }

            foreach (var root in rootMenus)
            {
                AddChildren(root);
            }

            return rootMenus;
        }
    }
}
