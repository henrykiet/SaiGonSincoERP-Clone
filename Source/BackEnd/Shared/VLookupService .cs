using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace reportSystem01.Shared
{
    [XmlRoot("VLookup")]
    public class VLookupConfiguration
    {
        [XmlElement("Lookup")]
        public List<VLookup> Lookups { get; set; }
    }

    public class VLookup
    {
        [XmlAttribute("code")]
        public string Code { get; set; }

        [XmlElement("Table")]
        public string Table { get; set; }

        [XmlArray("Fields")]
        [XmlArrayItem("Field")]
        public List<string> Fields { get; set; }

        [XmlElement("Where")]
        public string Where { get; set; }
    }
    public class VLookupService
    {
        private readonly string _lookupDirectoryPath;

        
        public async Task<VLookup> GetVLookupByCodeAsync(string code)
        {
            // Tìm tất cả các file XML trong thư mục lookup
            string baseDirectory = Directory.GetCurrentDirectory();
            string _vlookupFilePath = Path.Combine(baseDirectory, "Controllers", "VLookup", code+".xml");
            //var xmlFiles = Directory.GetFiles(_vlookupFilePath);

            string xmlContent = await File.ReadAllTextAsync(_vlookupFilePath);
            XmlSerializer serializer = new XmlSerializer(typeof(VLookupConfiguration));
            using (StringReader reader = new StringReader(xmlContent))
            {
                var config = (VLookupConfiguration)serializer.Deserialize(reader);
                var lookup = config.Lookups.FirstOrDefault(l => l.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase));
                if (lookup != null)
                {
                    return lookup;
                }
            }

            // Trả về null nếu không tìm thấy Lookup nào phù hợp
            return null;
        }
    }
}
