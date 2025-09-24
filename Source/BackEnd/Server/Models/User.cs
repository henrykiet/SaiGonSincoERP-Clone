using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Models;

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

    public virtual ICollection<ApiLog> ApiLogs { get; set; } = new List<ApiLog>();

    public virtual ICollection<Report1> Report1s { get; set; } = new List<Report1>();

    public virtual ICollection<UserLog> UserLogs { get; set; } = new List<UserLog>();
}
