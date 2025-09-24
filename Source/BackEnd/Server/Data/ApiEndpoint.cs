using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace reportSystem01.Server.Data;

public partial class ApiEndpoints
{
    [Key]
    public int EndpointId { get; set; }

    public string? EndpointUrl { get; set; }

    public string? Method { get; set; }

    public bool? IsSecure { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ApiLogs> ApiLogs { get; set; } = new List<ApiLogs>();
}
