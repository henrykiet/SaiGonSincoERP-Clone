namespace Sinco.Server.SqlJsonDefinations.Requests
{
	public class DataSaveRequest
	{
		public string FormId { get; set; }
		public string Action { get; set; }
		public SaveData Data { get; set; }
		public class SaveData
		{
			public string OrderCode { get; set; }
			public DateTime OrderDate { get; set; }
			public List<Detail> Details { get; set; }
		}
		public class Detail
		{
			public string Product { get; set; }
			public int Quantity { get; set; }
		}
	}
}
