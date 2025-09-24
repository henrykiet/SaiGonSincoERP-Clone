using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace reportSystem01.Server.Models;

public partial class ApiLog
{
    [Key]
    public int LogId { get; set; }

    public int? EndpointId { get; set; }

    public int? UserId { get; set; }

    public DateTime Datetime0 { get; set; }

    public string? Description { get; set; }

    public int? StatusCode { get; set; }

    public string? IpAddress { get; set; }

    public virtual ApiEndpoint? Endpoint { get; set; }

    public virtual User? User { get; set; }
}
