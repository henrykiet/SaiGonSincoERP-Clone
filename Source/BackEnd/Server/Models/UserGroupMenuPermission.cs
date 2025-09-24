using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Models;

public partial class UserGroupMenuPermission
{
    public int? UserGroupId { get; set; }

    public string? MenuId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? RAccess { get; set; }

    public int? RInsert { get; set; }

    public int? RUpdate { get; set; }

    public int? RDel { get; set; }

    public virtual Menu? Menu { get; set; }
}
