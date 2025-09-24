using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class Report
{
    public string Id { get; set; } = null!;

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime CreatedDate { get; set; }
}
