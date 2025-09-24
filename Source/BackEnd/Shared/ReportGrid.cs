using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace reportSystem01.Shared
{
    public class ReportGrid
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

       
        public List<ReportGrid> GetBrowserFromXml(string sysID)
        {
            //string reportFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Filter", sysID + ".xml");
            string baseDirectory = Directory.GetCurrentDirectory();
            string reportFilePath = Path.Combine(baseDirectory, "Controllers", "Browser", sysID + ".xml");
            var grids = new List<ReportGrid>();

            try
            {
                // Đọc file XML từ đường dẫn
                var xmlDoc = XDocument.Load(reportFilePath);

                // Trích xuất các filter từ XML
                var filterElements = xmlDoc.Descendants("Grid");

                foreach (var filterElement in filterElements)
                {
                    grids.Add(new ReportGrid
                    {
                        ID = filterElement.Attribute("id")?.Value,
                        Name = filterElement.Attribute("name")?.Value,
                        Type = filterElement.Attribute("type")?.Value
                    });
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (nếu có)
                Console.WriteLine($"Error reading XML: {ex.Message}");

            }

            return grids;
        }

        
    }
}
