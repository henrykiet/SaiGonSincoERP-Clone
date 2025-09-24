namespace Sinco.Server.Models
{
    public class CustomQueryRequest
    {
        public string UserId { get; set; }
        public string Unit { get; set; }
        public string Language { get; set; }
        public List<string> Params { get; set; }
        public List<string> DataType { get; set; }
        public List<string> Value { get; set; }
        public string Query { get; set; }
    }
}
