using AutoMapper;
using Microsoft.EntityFrameworkCore;
using reportSystem01.Server.Data;
using reportSystem01.Shared;

namespace Sinco.Server.Repositories.Auth
{
    public class UserGroupService : IUserGroupService
    {
        private readonly ReportServerContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserGroupService(ReportServerContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<int>> CreateUserGroup(UserGroupDto request)
        {
            var response = new ServiceResponse<int>();
            try
            {
                if (await UserGroupExists(request.GroupName))
                {
                    response.Success = false;
                    response.Message = "User Group đã tồn tại";
                    return response;
                }

                if (request.ListUser == "")
                {
                    response.Success = false;
                    response.Message = "Danh sách User không được trống";
                    return response;
                }
                int maxId = await _context.UserGroups.AnyAsync() ? await _context.UserGroups.MaxAsync(x => x.UserGroupId) : 0;
                var userGroup = new UserGroup
                {
                    UserGroupId = maxId + 1,
                    GroupName = request.GroupName,
                    Description = request.Description,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ListUser = request.ListUser
                };

                _context.UserGroups.Add(userGroup);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Log the actual exception
                    Console.WriteLine($"Database Error: {ex.InnerException?.Message}");
                    throw;
                }

                response.Data = userGroup.UserGroupId;
                response.Success = true;
                response.Message = "Tạo User Group thành công!";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }

        }

        public async Task<ServiceResponse<UserGroupDto>> GetUserGroupById(int id)
        {
            var response = new ServiceResponse<UserGroupDto>();
            try
            {
                var userGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(x => x.UserGroupId == id && !x.IsDeleted);

                if (userGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm người dùng";
                    return response;
                }

                response.Data = _mapper.Map<UserGroupDto>(userGroup);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<UserGroupDto>>> GetAllUserGroups()
        {
            var response = new ServiceResponse<List<UserGroupDto>>();
            try
            {
                var userGroups = await _context.UserGroups
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.UserGroupId)
                    .ToListAsync();

                response.Data = _mapper.Map<List<UserGroupDto>>(userGroups);
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<UserGroupDto>> UpdateUserGroup(UserGroupDto request)
        {
            var response = new ServiceResponse<UserGroupDto>();
            try
            {
                var userGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(x => x.UserGroupId == request.UserGroupId && !x.IsDeleted);

                if (userGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm người dùng";
                    return response;
                }

                // Kiểm tra tên nhóm đã tồn tại chưa (trừ chính nó)
                if (await _context.UserGroups.AnyAsync(x =>
                    x.GroupName.ToLower() == request.GroupName.ToLower() &&
                    x.UserGroupId != request.UserGroupId &&
                    !x.IsDeleted))
                {
                    response.Success = false;
                    response.Message = "Tên nhóm đã tồn tại";
                    return response;
                }

                userGroup.GroupName = request.GroupName;
                userGroup.Description = request.Description;
                userGroup.ListUser = request.ListUser;
                userGroup.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<UserGroupDto>(userGroup);
                response.Success = true;
                response.Message = "Cập nhật nhóm người dùng thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteUserGroup(int id)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var userGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(x => x.UserGroupId == id && !x.IsDeleted);

                if (userGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm người dùng";
                    return response;
                }

                userGroup.IsDeleted = true;
                userGroup.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "Xóa nhóm người dùng thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> UpdateTreeViewPermissions(TreeViewPermissionDto request)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var userGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(x => x.UserGroupId == request.UserGroupId && !x.IsDeleted);

                if (userGroup == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy nhóm người dùng";
                    return response;
                }

                //userGroup.TreeViewPermissions = request.Permissions;
                userGroup.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Success = true;
                response.Message = "Cập nhật phân quyền thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<bool> UserGroupExists(string GroupName)
        {
            return await _context.UserGroups.AnyAsync(x => x.GroupName.ToLower() == GroupName.ToLower());
        }
    }
}
