namespace Sinco.Server.SqlJsonDefinations.Requests
{
	public class DataLookupRequest
	{
		public string TableName { get; set; }
		public string SearchTerm { get; set; }
		public DataParam Param { get; set; }
		public class DataParam
		{
			public DateTime? StartDate { get; set; }
			public DateTime? EndDate { get; set; }
		}
	}
}
