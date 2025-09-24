using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class UserGroupPermission
{
    public int? UserGroupId { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual UserGroup? UserGroup { get; set; }
}
