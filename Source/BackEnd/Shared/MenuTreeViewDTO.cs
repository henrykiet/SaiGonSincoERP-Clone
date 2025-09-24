using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class MenuTreeViewDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? ParentId { get; set; }
        public bool IsChecked { get; set; }
        public List<MenuTreeViewDTO> Children { get; set; } = new List<MenuTreeViewDTO>();
    }
}
