using System;
using System.ComponentModel.DataAnnotations;
namespace reportSystem01.Shared
{
    public class ReportConnection
    {
        public int ConnectionId { get; set; }

        [Required(ErrorMessage = "Tên kết nối là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên kết nối tối đa 100 ký tự")]
        public string ConnectionName { get; set; }

        [Required(ErrorMessage = "Loại CSDL là bắt buộc")]
        [StringLength(50, ErrorMessage = "Loại CSDL tối đa 50 ký tự")]
        public string DbType { get; set; }

        [Required(ErrorMessage = "Chuỗi kết nối là bắt buộc")]
        [StringLength(200, ErrorMessage = "Chuỗi kết nối tối đa 200 ký tự")]
        public string ConnectionString { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
