using System.ComponentModel.DataAnnotations;

namespace Sinco.Server.Models
{
    public class Unit
    {
        [Key]
        [StringLength(16)]
        public string UnitCode { get; set; } = string.Empty;
        
        [StringLength(128)]
        public string UnitName { get; set; } = string.Empty;
        
        [StringLength(128)]
        public string UnitName2 { get; set; } = string.Empty;
    }

    public class UnitDto
    {
        public string UnitCode { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string UnitName2 { get; set; } = string.Empty;
    }
} 