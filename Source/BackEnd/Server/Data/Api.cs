using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class Api
{
    public int ApiId { get; set; }

    public string ApiName { get; set; } = null!;

    public string ApiUrl { get; set; } = null!;

    public string ApiMethod { get; set; } = null!;

    public string? AuthenticationType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ApiLogs> ApiLogs { get; set; } = new List<ApiLogs>();
}
