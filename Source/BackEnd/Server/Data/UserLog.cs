using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace reportSystem01.Server.Data;

public partial class UserLogs
{
    [Key]
    public int LogId { get; set; }

    public int? UserId { get; set; }

    public string? Action { get; set; }

    public DateTime ActionTime { get; set; }

    public string? IpAddress { get; set; }

    public string? Description { get; set; }

    public virtual User? User { get; set; }
}
