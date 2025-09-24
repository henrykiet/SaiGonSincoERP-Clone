using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Models;

public partial class UserLog
{
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime ActionTime { get; set; }

    public string? IpAddress { get; set; }

    public string? Description { get; set; }

    public virtual User? User { get; set; }
}
