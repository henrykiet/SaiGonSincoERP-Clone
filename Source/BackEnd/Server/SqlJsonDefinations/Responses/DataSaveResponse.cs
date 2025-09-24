namespace Sinco.Server.SqlJsonDefinations.Responses
{
	public class DataSaveResponse
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public List<Error>? Errors { get; set; }
		public class Error
		{
			public string Field { get; set; }
			public string Mesage { get; set; }
		}
	}
}
