using System.Xml.Linq;
using System.Xml.Serialization;

namespace reportSystem01.Shared;

public class ReportFilter
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Valid { get; set; }
    public int ListPageIndex { get; set; } = 1;
    public int ListPageSize { get; set; } = 5;

    // Tính số trang
    public int TotalPages => (ListBoxItems.Count + ListPageSize - 1) / ListPageSize;


    // Thuộc tính cho listbox
    public List<ListBoxItem> ListBoxItems { get; set; } = new List<ListBoxItem>();
    public string GetStoreProcName(string sysID)
    {
        // Đọc các filter từ file XML
        //string reportFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Filter", sysID + ".xml");
        string baseDirectory = Directory.GetCurrentDirectory();
        string reportFilePath = Path.Combine(baseDirectory, "Controllers", "Filter", sysID + ".xml");
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
    public List<ReportFilter> GetFiltersFromXml(string sysID)
    {
        //string reportFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Filter", sysID + ".xml");
        string baseDirectory = Directory.GetCurrentDirectory();
        string reportFilePath = Path.Combine(baseDirectory, "Controllers", "Filter", sysID + ".xml");
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
                    ID = filterElement.Attribute("id")?.Value,
                    Name = filterElement.Attribute("name")?.Value,
                    Type = filterElement.Attribute("type")?.Value,
                    Valid = filterElement.Attribute("valid")?.Value
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
    public async Task<VLookup> GetVLookupByCodeAsync(string code)
    {
        string baseDirectory = Directory.GetCurrentDirectory();
        string _vlookupFilePath = Path.Combine(baseDirectory, "Controllers", "Filter", "vlookups.xml");
        if (!File.Exists(_vlookupFilePath))
            return null;

        string xmlContent = await File.ReadAllTextAsync(_vlookupFilePath);
        XmlSerializer serializer = new XmlSerializer(typeof(VLookupConfiguration));
        using (StringReader reader = new StringReader(xmlContent))
        {
            var config = (VLookupConfiguration)serializer.Deserialize(reader);
            return config.Lookups.FirstOrDefault(l => l.Code.Equals(code, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
