using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class ReportConnection
{
    public int ConnectionId { get; set; }

    public string? ConnectionName { get; set; }

    public string? DbType { get; set; }

    public string? ConnectionString { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
