using reportSystem01.Shared;
using System.Security.Cryptography;
using System.Text;
using System;
using reportSystem01.Server.Data;
using Microsoft.EntityFrameworkCore;
using User = reportSystem01.Server.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Sinco.Server.Repositories.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ReportServerContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(ReportServerContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<int>> Register(UserRegisterDto request)
        {
            var response = new ServiceResponse<int>();

            try
            {
                if (await UserExists(request.UserName))
                {
                    response.Success = false;
                    response.Message = "Username đã tồn tại";
                    return response;
                }

                if (await EmailExists(request.Email))
                {
                    response.Success = false;
                    response.Message = "Email đã được sử dụng";
                    return response;
                }

                CreatePasswordHash(request.Password,
                    out byte[] passwordHash,
                    out byte[] passwordSalt);

                var user = new User.User
                {
                    UserName = request.UserName,
                    FullName = request.FullName,
                    Email = request.Email,
                    PasswordHash = ConvertByteToString(passwordHash),
                    PasswordSalt = ConvertByteToString(passwordSalt),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Role = "User"
                };

                _context.Users.Add(user);

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

                response.Data = user.UserId;
                response.Success = true;
                response.Message = "Tạo tài khoản thành công!";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(u =>
                u.UserName.ToLower() == username.ToLower());
        }

        public async Task<bool> EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                return await _context.Users.AnyAsync(u =>
                    u.Email.ToLower().Trim() == email.ToLower().Trim());
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public string ConvertByteToString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        // Chuyển ngược lại từ string sang byte[]
        public byte[] ConvertStringToByte(string str)
        {
            return Convert.FromBase64String(str);
        }


        public async Task<ServiceResponseLogin<LoginResponseDto>> Login(UserLoginDto request)
        {
            var response = new ServiceResponseLogin<LoginResponseDto>();
            string sessionId = Guid.NewGuid().ToString();
            
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == request.UserName.ToLower() && u.IsLocked == false && u.IsDeleted == false);

                if (user == null)
                {
                    // Log failed login attempt
                    await LogUserLogin(0, request.UserName, sessionId, request.IpAddress ?? "", request.UserAgent ?? "", "Failed", request.Unit ?? "", "User không tồn tại");
                    
                    response.Success = false;
                    response.Message = "User hoặc Password không đúng.";
                    return response;
                }

                if (!await VerifyPasswordHash(request.Password, ConvertStringToByte(user.PasswordHash), ConvertStringToByte(user.PasswordSalt)))
                {
                    // Log failed login attempt
                    await LogUserLogin(user.UserId, user.UserName, sessionId, request.IpAddress ?? "", request.UserAgent ?? "", "Failed", request.Unit ?? "", "Sai mật khẩu");
                    
                    response.Success = false;
                    response.Message = "User hoặc Password không đúng.";
                    return response;
                }

                // Kiểm tra session hiện tại và invalidate nếu cần
                bool hasExistingSession = false;
                if (!string.IsNullOrEmpty(user.CurrentSessionId))
                {
                    hasExistingSession = true;
                    Console.WriteLine($"🔄 User {user.UserName} already has session: {user.CurrentSessionId}. Invalidating...");
                    
                    // Log out session cũ
                    await LogUserLogout(user.CurrentSessionId);
                }

                // Cập nhật session mới cho user
                Console.WriteLine($"✅ Setting new session for user {user.UserName}: {sessionId}");
                user.CurrentSessionId = sessionId;
                user.CurrentSessionCreated = DateTime.Now;
                user.LastLoginTime = DateTime.Now;
                
                await _context.SaveChangesAsync();
                Console.WriteLine($"💾 Session saved to database for user {user.UserName}");

                // Log successful login
                await LogUserLogin(user.UserId, user.UserName, sessionId, request.IpAddress ?? "", request.UserAgent ?? "", "Success", request.Unit ?? "");

                // Tạo response object chứa cả token và unit
                var loginData = new LoginResponseDto
                {
                    Token = CreateToken(user, sessionId),
                    Unit = request.Unit,
                    UserId = user.UserId,
                    UserName = user.UserName,
                    SessionId = sessionId
                };

                response.Data = loginData;
                response.Message = hasExistingSession ? "Đăng nhập thành công. Session trước đã được đăng xuất." : "Đăng nhập thành công";
                response.HasExistingSession = hasExistingSession;
                
                return response;
            }
            catch (Exception ex)
            {
                // Log system error
                await LogUserLogin(0, request.UserName, sessionId, request.IpAddress ?? "", request.UserAgent ?? "", "Failed", request.Unit ?? "", $"System error: {ex.Message}");
                
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }

        public async Task<bool> VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public string CreateToken(User.User user, string sessionId)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email ?? ""),
                new(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("Id", user.UserId.ToString()),
                new Claim("SessionId", sessionId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(8), // Giảm thời gian token xuống 8 giờ
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public async Task<ServiceResponse<bool>> DeleteUser(int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy user.";
                    return response;
                }

                // Kiểm tra nếu user là Admin thì không cho xóa
                //if (user.Role?.ToLower() == "admin")
                //{
                //    response.Success = false;
                //    response.Message = "Không thể xóa tài khoản Admin.";
                //    return response;
                //}

                //_context.Users.Remove(user);
                user.IsDeleted = true;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Message = "Xóa user thành công.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi xóa user: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> UpdateUser(int userId, UpdateUserDto request)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy user.";
                    return response;
                }

                user.FullName = request.FullName;
                user.Email = request.Email;
                user.UpdatedAt = DateTime.Now;
                user.IsLocked = request.IsLocked;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Message = "Update user thành công.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi update user: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy user.";
                    return response;
                }

                // Verify current password
                if (!await VerifyPasswordHash(currentPassword,
                    ConvertStringToByte(user.PasswordHash),
                    ConvertStringToByte(user.PasswordSalt)))
                {
                    response.Success = false;
                    response.Message = "Mật khẩu hiện tại không đúng.";
                    return response;
                }

                // Create new password hash
                CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

                // Update password
                user.PasswordHash = ConvertByteToString(passwordHash);
                user.PasswordSalt = ConvertByteToString(passwordSalt);
                user.UpdatedAt = DateTime.Now;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                response.Data = true;
                response.Message = "Đổi mật khẩu thành công.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi đổi mật khẩu: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserById(int userId)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsDeleted == false);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User không tồn tại.";
                    return response;
                }

                response.Data = _mapper.Map<UserDto>(user);
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<List<UserDto>>> GetAllUsers()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _context.Users
                    .Where(e => e.Role != "Admin" && e.IsDeleted == false)
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                response.Data = _mapper.Map<List<UserDto>>(users);
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<List<UserDto>>> SearchUsers(string searchText)
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _context.Users
                    .Where(u => (u.UserName.ToLower().Contains(searchText.ToLower()) ||
                               u.Email.ToLower().Contains(searchText.ToLower())) && u.IsDeleted == false)
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                response.Data = _mapper.Map<List<UserDto>>(users);
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi: {ex.Message}";
                return response;
            }
        }

        public async Task<ServiceResponse<List<UserDto>>> GetUsersByNames(IEnumerable<string> userNames)
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _context.Users
                    //.Where(u => userNames.Contains(u.UserName))
                    .Where(u => u.Role != "Admin")
                    .Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        Email = u.Email
                    }).ToListAsync();

                response.Data = _mapper.Map<List<UserDto>>(users);
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi: {ex.Message}";
                return response;
            }
        }

        //public async Task<ServiceResponse<List<UserDto>>> GetUsersByRole(string role)
        //{
        //    var response = new ServiceResponse<List<UserDto>>();
        //    try
        //    {
        //        var users = await _context.Users
        //            .Where(u => u.Role.ToLower() == role.ToLower())
        //            .OrderBy(u => u.Username)
        //            .ToListAsync();

        //        response.Data = _mapper.Map<List<UserDto>>(users);
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Success = false;
        //        response.Message = $"Lỗi: {ex.Message}";
        //        return response;
        //    }
        //}

        /// <summary>
        /// Ghi log đăng nhập của user
        /// </summary>
        public async Task<ServiceResponse<bool>> LogUserLogin(int userId, string userName, string sessionId, string ipAddress, string userAgent, string status, string unit, string? failureReason = null)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var loginLog = new LoginLog
                {
                    UserId = userId,
                    UserName = userName,
                    LoginTime = DateTime.Now,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    SessionId = sessionId,
                    Status = status,
                    FailureReason = failureReason,
                    Unit = unit
                };

                _context.Set<LoginLog>().Add(loginLog);
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi ghi log đăng nhập: {ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// Ghi log đăng xuất của user
        /// </summary>
        public async Task<ServiceResponse<bool>> LogUserLogout(string sessionId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var loginLog = await _context.Set<LoginLog>()
                    .FirstOrDefaultAsync(l => l.SessionId == sessionId && l.Status == "Success");

                if (loginLog != null)
                {
                    loginLog.LogoutTime = DateTime.Now;
                    loginLog.Status = "Logout";
                    await _context.SaveChangesAsync();
                }

                response.Success = true;
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi ghi log đăng xuất: {ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// Hủy tất cả session của user trừ session hiện tại
        /// </summary>
        public async Task<ServiceResponse<bool>> InvalidateUserSessions(int userId, string currentSessionId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user != null && user.CurrentSessionId != currentSessionId)
                {
                    // Log out old session
                    if (!string.IsNullOrEmpty(user.CurrentSessionId))
                    {
                        await LogUserLogout(user.CurrentSessionId);
                    }

                    // Clear old session
                    user.CurrentSessionId = null;
                    user.CurrentSessionCreated = null;
                    await _context.SaveChangesAsync();
                }

                response.Success = true;
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi hủy session: {ex.Message}";
                return response;
            }
        }
    }
}


