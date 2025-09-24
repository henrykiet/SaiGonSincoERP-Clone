using System;

namespace Sinco.Server.Models
{
    public class FileAttachment
    {
        public string Controller { get; set; }
        public string SysKey { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string ContentType { get; set; }
    }
} 