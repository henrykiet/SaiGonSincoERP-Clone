using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "FullName là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password phải từ 6 ký tự trở lên")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Password không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
