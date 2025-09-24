using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class MenuDto
    {
        public string MenuId { get; set; }
        public string Name { get; set; }
        public string? Name2 { get; set; }
        public string ParentMenuId { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string VoucherCode { get; set; }
        public string Type { get; set; }
        public bool HasAccess { get; set; }
        public bool HasInsert { get; set; }
        public bool HasUpdate { get; set; }
        public bool HasDel { get; set; }
        public List<MenuDto> Children { get; set; } = new();
    }
}
