using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using reportSystem01.Server.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Sinco.Server.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            Console.WriteLine($"🔍 SessionValidationMiddleware: Processing request to {path}");

            // Bỏ qua validation cho các endpoint không cần authentication
            if (ShouldSkipValidation(context))
            {
                Console.WriteLine($"⏭️  SessionValidationMiddleware: Skipping validation for {path}");
                await _next(context);
                return;
            }

            // Kiểm tra Authorization header
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Console.WriteLine($"🚫 SessionValidationMiddleware: No Bearer token found for {path}");
                await _next(context);
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            Console.WriteLine($"🎫 SessionValidationMiddleware: Found Bearer token for {path}");
            
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                var sessionIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "SessionId");

                Console.WriteLine($"🔑 SessionValidationMiddleware: UserId={userIdClaim?.Value}, SessionId={sessionIdClaim?.Value}");

                if (userIdClaim == null || sessionIdClaim == null)
                {
                    Console.WriteLine($"❌ SessionValidationMiddleware: Missing claims in token for {path}");
                    await _next(context);
                    return;
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine($"❌ SessionValidationMiddleware: Invalid userId format for {path}");
                    await _next(context);
                    return;
                }

                // Kiểm tra session trong database
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ReportServerContext>();
                
                var user = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted && !u.IsLocked);

                Console.WriteLine($"👤 SessionValidationMiddleware: Found user {user?.UserName}, CurrentSessionId={user?.CurrentSessionId}");

                if (user == null)
                {
                    Console.WriteLine($"❌ User not found or deleted/locked: {userId}");
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"User không tồn tại hoặc bị khóa. Vui lòng đăng nhập lại.\"}");
                    return;
                }

                if (user.CurrentSessionId != sessionIdClaim.Value)
                {
                    Console.WriteLine($"❌ INVALID SESSION for user {userId} ({user.UserName}). Expected: {user.CurrentSessionId}, Got: {sessionIdClaim.Value}");
                    
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"Session không hợp lệ. Vui lòng đăng nhập lại.\"}");
                    return;
                }

                Console.WriteLine($"✅ VALID SESSION for user {userId} ({user.UserName})");

                // Kiểm tra session có quá cũ không (tùy chọn - có thể set timeout cho session)
                if (user.CurrentSessionCreated.HasValue)
                {
                    var sessionAge = DateTime.Now - user.CurrentSessionCreated.Value;
                    if (sessionAge.TotalHours > 24) // Session timeout sau 24 giờ
                    {
                        _logger.LogWarning($"Session expired due to age for user {userId}. Age: {sessionAge.TotalHours} hours");
                        
                        // Clear expired session
                        user.CurrentSessionId = null;
                        user.CurrentSessionCreated = null;
                        await dbContext.SaveChangesAsync();
                        
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"message\": \"Session đã hết hạn. Vui lòng đăng nhập lại.\"}");
                        return;
                    }
                }

                // Session hợp lệ, tiếp tục
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session");
                await _next(context);
            }
        }

        private bool ShouldSkipValidation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            
            // Bỏ qua các endpoint public
            var skipPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/swagger",
                "/health",
                "/"
            };

            if (path != null && skipPaths.Any(skipPath => path.StartsWith(skipPath)))
            {
                return true;
            }

            // Kiểm tra nếu endpoint có [AllowAnonymous]
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            {
                return true;
            }

            // Chỉ validate session cho các API endpoints (không phải static files)
            if (path != null && !path.StartsWith("/api/"))
            {
                return true;
            }

            return false;
        }
    }

    public static class SessionValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionValidationMiddleware>();
        }
    }
}
