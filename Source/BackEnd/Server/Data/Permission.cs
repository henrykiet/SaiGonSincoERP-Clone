using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string PermissionName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<UserGroup> Groups { get; set; } = new List<UserGroup>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
