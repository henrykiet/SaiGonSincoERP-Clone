namespace reportSystem01.Shared
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }
} 