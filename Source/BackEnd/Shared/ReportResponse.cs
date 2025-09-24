using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{


    public class ReportResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<Dictionary<string, object>> Reports { get; set; }
        public List<Dictionary<string, object>> MasterData { get; set; }

    }
}
