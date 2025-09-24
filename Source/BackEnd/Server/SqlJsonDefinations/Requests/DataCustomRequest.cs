namespace Sinco.Server.SqlJsonDefinations.Requests
{
	public class DataCustomRequest
	{
		public string FormId { get; set; }
		public string Action { get; set; }
		public CustomData Data { get; set; }
		public class CustomData
		{
			public int Amount { get; set; }
		}
	}
}
