using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class FilterModel
    {
        public Dictionary<string, object> Filters { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
