using System.Xml.Linq;

namespace reportSystem01.Server.Models
{
    public class ReportFilter
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public string GetStoreProcName(string reportFilePath)
        {
            try
            {
                // Đọc file XML
                var xmlDoc = XDocument.Load(reportFilePath);

                // Lấy tên storeProc từ XML
                var storeProc = xmlDoc.Element("Report")?.Element("storeProc")?.Value;

                return storeProc;  // Trả về tên stored procedure
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading XML: {ex.Message}");
                return null;
            }
        }
        public List<ReportFilter> GetFiltersFromXml(string reportFilePath)
        {
            var filters = new List<ReportFilter>();

            try
            {
                // Đọc file XML từ đường dẫn
                var xmlDoc = XDocument.Load(reportFilePath);

                // Trích xuất các filter từ XML
                var filterElements = xmlDoc.Descendants("Filter");

                foreach (var filterElement in filterElements)
                {
                    filters.Add(new ReportFilter
                    {
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

            return filters;
        }
    }
}
