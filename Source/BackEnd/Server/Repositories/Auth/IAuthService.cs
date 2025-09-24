using reportSystem01.Server.Data;
using reportSystem01.Shared;
using User = reportSystem01.Server.Data.User;

namespace Sinco.Server.Repositories.Auth
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(UserRegisterDto request);
        Task<ServiceResponseLogin<LoginResponseDto>> Login(UserLoginDto request);
        Task<bool> UserExists(string username);
        Task<bool> EmailExists(string email);
        Task<ServiceResponse<bool>> DeleteUser(int userId);
        Task<ServiceResponse<bool>> UpdateUser(int userId, UpdateUserDto request);
        Task<ServiceResponse<bool>> ChangePassword(int userId, string currentPassword, string newPassword);
        Task<ServiceResponse<UserDto>> GetUserById(int userId);
        Task<ServiceResponse<List<UserDto>>> GetAllUsers();
        Task<ServiceResponse<List<UserDto>>> SearchUsers(string searchText);
        Task<ServiceResponse<List<UserDto>>> GetUsersByNames(IEnumerable<string> userNames);
        //Task<ServiceResponse<List<UserDto>>> GetUsersByRole(string role);
        Task<bool> VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateToken(User user, string sessionId);
        Task<ServiceResponse<bool>> LogUserLogin(int userId, string userName, string sessionId, string ipAddress, string userAgent, string status, string unit, string? failureReason = null);
        Task<ServiceResponse<bool>> LogUserLogout(string sessionId);
        Task<ServiceResponse<bool>> InvalidateUserSessions(int userId, string currentSessionId);
    }
}
