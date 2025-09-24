using System;
using System.Collections.Generic;

namespace reportSystem01.Server.Models;

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
}
