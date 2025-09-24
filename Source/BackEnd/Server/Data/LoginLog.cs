using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace reportSystem01.Server.Data
{
    [Table("LoginLogs")]
    public class LoginLog
    {
        [Key]
        public int LoginLogId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = null!;

        [Required]
        public DateTime LoginTime { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = null!; // "Success", "Failed", "Logout"

        [StringLength(500)]
        public string? FailureReason { get; set; }

        public DateTime? LogoutTime { get; set; }

        [StringLength(100)]
        public string? Unit { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
