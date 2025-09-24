namespace Sinco.Server.SqlJsonDefinations.Responses
{
	public class DataCustomResponse
	{
		public bool Success { get; set; }
		public DataResult Result { get; set; }
		public class DataResult
		{
			public int Tax { get; set; }
		}
	}
}
