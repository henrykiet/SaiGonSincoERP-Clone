using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Email { get; set; }

    public bool IsLocked { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public bool IsDeleted { get; set; }

    public string PasswordSalt { get; set; } = null!;

    public string? Role { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Session ID hiện tại của user (để kiểm tra single login)
    /// </summary>
    public string? CurrentSessionId { get; set; }

    /// <summary>
    /// Thời gian session hiện tại được tạo
    /// </summary>
    public DateTime? CurrentSessionCreated { get; set; }

    public virtual ICollection<ApiLogs> ApiLogs { get; set; } = new List<ApiLogs>();

    public virtual ICollection<Report1> Report1s { get; set; } = new List<Report1>();

    public virtual ICollection<UserLogs> UserLogs { get; set; } = new List<UserLogs>();

    public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
}
