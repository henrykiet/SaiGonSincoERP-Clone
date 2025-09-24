using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace reportSystem01.Server.Models;

public partial class ApiEndpoint
{
    [Key]
    public int EndpointId { get; set; }

    public string? EndpointUrl { get; set; }

    public string? Method { get; set; }

    public bool? IsSecure { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ApiLog> ApiLogs { get; set; } = new List<ApiLog>();
}
