using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "User Name bắt buộc nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pass word bắt buộc nhập")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị cơ sở bắt buộc nhập")]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// IP Address của client
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent của trình duyệt
        /// </summary>
        public string? UserAgent { get; set; }
    }
}
