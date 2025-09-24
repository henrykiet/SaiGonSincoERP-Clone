namespace Sinco.Server.SqlJsonDefinations.Responses
{
	public class DataListResponse
	{
		public List<Data> Datas { get; set; }
		public int Total { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public class Data
		{
			public string OrderCode { get; set; }
			public DateTime OrderDate { get; set; }
			public string Status { get; set; }
		}
	}
}
