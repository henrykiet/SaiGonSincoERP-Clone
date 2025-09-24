using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class SavePermissionRequestDTO
    {
        public int UserId { get; set; }
        public List<MenuPermissionDTO> Permissions { get; set; }
    }
}
