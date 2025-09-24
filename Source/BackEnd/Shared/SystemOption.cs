using System;
using System.Collections.Generic;

namespace reportSystem01.Shared;

public partial class SystemOption
{
    public int Id { get; set; }

    public string ConfigKey { get; set; } = null!;

    public string ConfigValue { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string userGroup { get; set; } = null!;
}
