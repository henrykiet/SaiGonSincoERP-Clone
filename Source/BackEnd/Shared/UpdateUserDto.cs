using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        //public string Role { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}
