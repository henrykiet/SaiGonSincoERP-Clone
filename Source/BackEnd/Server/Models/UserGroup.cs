using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Models;

public partial class UserGroup
{
    public int UserGroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
