using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Data;

public partial class Menu
{
    public string MenuId { get; set; } = null!;

    public string MenuName { get; set; } = null!;

    public string? SysId { get; set; }

    public string? ParentMenuId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? TypeMenu { get; set; }

    public string? Icon { get; set; }

    public string? MenuName2 { get; set; }

    public string? VoucherCode { get; set; }

    public bool? isExpanded { get; set; }
}
