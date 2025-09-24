namespace Sinco.Server.SqlJsonDefinations.Requests
{
	public class DataListRequest
	{
		public string FormId { get; set; }
		public DataFilter Filter { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public string Sort { get; set; }
		public class DataFilter
		{
			public string Status { get; set; }
		}
	}
}
