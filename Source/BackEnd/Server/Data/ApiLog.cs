using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace reportSystem01.Server.Data;

public partial class ApiLogs
{
    [Key]
    public int LogId { get; set; }

    public int? EndpointId { get; set; }

    public int? UserId { get; set; }

    public DateTime datetime0 { get; set; }

    public string? Description { get; set; }

    public int? StatusCode { get; set; }

    public string? IpAddress { get; set; }

    public virtual ApiEndpoints? Endpoint { get; set; }

    public virtual User? User { get; set; }
}
