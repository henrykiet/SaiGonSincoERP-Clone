using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace reportSystem01.Server.Data;

public partial class Report1
{
    [Key]
    public int ReportId { get; set; }

    public string? ReportName { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? ScheduleTime { get; set; }

    public string? ReportType { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
