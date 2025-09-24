namespace Sinco.Server.SqlJsonDefinations.Requests
{
	public class DataDeleteRequest
	{
		public string FormId { get; set; }
        public string PrimaryKey { get; set; }
        public List<string> Ids { get; set; }
        public string Status { get; set; }
        public string? Action { get; set; }
    }
}
