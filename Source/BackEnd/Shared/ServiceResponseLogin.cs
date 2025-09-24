using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public class ServiceResponseLogin<T>
    {
        public T? Data { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public bool HasExistingSession { get; set; } = false;
    }
}
