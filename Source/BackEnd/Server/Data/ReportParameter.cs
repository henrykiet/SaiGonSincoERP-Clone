using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class ReportParameter
{
    public int ParameterId { get; set; }

    public int? ReportId { get; set; }

    public string ParameterName { get; set; } = null!;

    public string ParameterType { get; set; } = null!;

    public string? DefaultValue { get; set; }

    public bool? IsRequired { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Report1? Report { get; set; }
}
