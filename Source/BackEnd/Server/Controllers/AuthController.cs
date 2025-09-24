using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using reportSystem01.Shared;
using Sinco.Server.Repositories.Auth;

namespace reportSystem01.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
        {
            var response = await _authService.Register(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login")]
        //[Authorize]
        public async Task<ActionResult<ServiceResponseLogin<LoginResponseDto>>> Login(UserLoginDto request)
        {
            // Lấy IP Address và User Agent từ request
            request.IpAddress = GetClientIpAddress();
            request.UserAgent = Request.Headers["User-Agent"].ToString();

            var response = await _authService.Login(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        /// <summary>
        /// Lấy IP Address của client
        /// </summary>
        private string GetClientIpAddress()
        {
            // Kiểm tra header X-Forwarded-For (cho proxy/load balancer)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Kiểm tra header X-Real-IP
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Lấy từ connection
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        [HttpDelete("{id}")]
        [Authorize] // Removed Roles restriction for now
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteUser(int id)
        {
            var response = await _authService.DeleteUser(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<UserDto>>> GetUser(int id)
        {
            var response = await _authService.GetUserById(id);
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet]
        [Authorize] // Chỉ Admin mới có thể xem tất cả users
        public async Task<ActionResult<ServiceResponse<List<UserDto>>>> GetAllUsers()
        {
            var response = await _authService.GetAllUsers();
            return Ok(response);
        }

        [HttpGet("search/{searchText}")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<List<UserDto>>>> SearchUsers(string searchText)
        {
            var response = await _authService.SearchUsers(searchText);
            return Ok(response);
        }

        [HttpPut("users/{id}")]
        [Authorize] // Removed Roles restriction for now
        public async Task<ActionResult<ServiceResponse<bool>>> UpdateUser(int id, UpdateUserDto request)
        {
            var response = await _authService.UpdateUser(id, request);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("bynames")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<List<UserDto>>>> GetUsersByNames([FromBody] IEnumerable<string> userNames)
        {
            var response = await _authService.GetUsersByNames(userNames);
            return Ok(response);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var response = await _authService.ChangePassword(request.UserId, request.CurrentPassword, request.NewPassword);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        //[HttpGet("role/{role}")]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<ServiceResponse<List<UserDto>>>> GetUsersByRole(string role)
        //{
        //    var response = await _authService.GetUsersByRole(role);
        //    return Ok(response);
        //}

        [HttpGet("validate-session")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> ValidateSession()
        {
            var response = new ServiceResponse<bool>();
            
            try
            {
                // Session đã được validate qua SessionValidationMiddleware
                // Nếu đến đây nghĩa là session hợp lệ
                response.Data = true;
                response.Success = true;
                response.Message = "Session hợp lệ";
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi kiểm tra session: {ex.Message}";
                return BadRequest(response);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> Logout()
        {
            var response = new ServiceResponse<bool>();
            
            try
            {
                // Lấy thông tin user từ token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var sessionIdClaim = User.FindFirst("SessionId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Log logout
                    if (sessionIdClaim != null)
                    {
                        await _authService.LogUserLogout(sessionIdClaim.Value);
                    }

                    // Clear session trong database
                    await _authService.InvalidateUserSessions(userId, sessionIdClaim?.Value ?? "");
                }

                response.Data = true;
                response.Success = true;
                response.Message = "Đăng xuất thành công";
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Lỗi khi đăng xuất: {ex.Message}";
                return BadRequest(response);
            }
        }

        [HttpGet("test-middleware")]
        [Authorize]
        public async Task<ActionResult<object>> TestMiddleware()
        {
            Console.WriteLine("🧪 TEST: test-middleware endpoint called");
            return Ok(new { Message = "✅ Middleware passed, endpoint reached" });
        }

        [HttpGet("debug-session")]
        public async Task<ActionResult<object>> DebugSession()
        {
            Console.WriteLine("🔧 DEBUG: debug-session endpoint called");
            
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"🔧 DEBUG: Authorization header = {authHeader}");

                if (string.IsNullOrEmpty(authHeader))
                {
                    return Ok(new { Message = "❌ No Authorization header found" });
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var sessionIdClaim = User.FindFirst("SessionId");
                var userNameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);

                Console.WriteLine($"🔧 DEBUG: Claims - UserId={userIdClaim?.Value}, SessionId={sessionIdClaim?.Value}, UserName={userNameClaim?.Value}");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await _authService.GetUserById(userId);
                    Console.WriteLine($"🔧 DEBUG: Database CurrentSessionId = {user.Data?.CurrentSessionId}");
                    
                    return Ok(new
                    {
                        TokenUserId = userIdClaim?.Value,
                        TokenSessionId = sessionIdClaim?.Value,
                        TokenUserName = userNameClaim?.Value,
                        DatabaseCurrentSession = user.Data?.CurrentSessionId,
                        IsSessionValid = user.Data?.CurrentSessionId == sessionIdClaim?.Value,
                        Message = user.Data?.CurrentSessionId == sessionIdClaim?.Value 
                            ? "✅ Session hợp lệ" 
                            : $"❌ Session không hợp lệ. DB: {user.Data?.CurrentSessionId}, Token: {sessionIdClaim?.Value}"
                    });
                }

                return BadRequest("Không thể parse userId từ token");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔧 DEBUG: Exception = {ex.Message}");
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }
    }
}

