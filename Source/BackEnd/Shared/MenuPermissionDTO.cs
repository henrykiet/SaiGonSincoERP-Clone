using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class MenuPermissionDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentMenuId { get; set; } = string.Empty;
        public bool HasAccess { get; set; }
        public bool CanInsert { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool IsExpanded { get; set; }
        public HashSet<MenuPermissionDTO> Children { get; set; } = new();
    }
}
