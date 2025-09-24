namespace Sinco.Server.SqlJsonDefinations.Responses
{
	public class DataReportPivotResponse
	{
		public List<Row> Rows { get; set; }
		public class Row
		{
			public DateTime Month { get; set; }
			public int ProductA { get; set; }
			public int ProductB { get; set; }
		}
	}
}
