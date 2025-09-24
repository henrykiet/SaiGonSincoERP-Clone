using System.Text.Json;

namespace Sinco.Server.Models
{
    public class FormConfig
    {
        public string Title { get; set; }
        public string Class { get; set; }
        public List<TabConfig> Tabs { get; set; }
    }

    public class TabConfig
    {
        public string Title { get; set; }
        public string Class { get; set; }
        public FormSection Form { get; set; }
        public DetailSection Detail { get; set; }
    }

    public class FormSection
    {
        public string Title { get; set; }
        public string Class { get; set; }
        public List<FieldConfig> Fields { get; set; }
    }

    public class DetailSection
    {
        public string Title { get; set; }
        public string Entity { get; set; }
        public string ForeignKey { get; set; }
        public List<FieldConfig> Fields { get; set; }
        public List<JsonElement> InitialData { get; set; }
    }

    public class FieldConfig
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string Placeholder { get; set; }
        public bool Disabled { get; set; }
        public int? Min { get; set; }
        public object Default { get; set; }
    }
} 