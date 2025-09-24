namespace Sinco.Server.SqlJsonDefinations.Responses
{
	public class MetadataResponse
	{

		public string Controller { get; set; } = "";
		public string Type { get; set; } = "";
		public string FormId { get; set; } = "";
		public string PrimaryKey { get; set; } = "";
		public string Title { get; set; } = "";
		public string IdVC { get; set; } = "";
		public string VCDate { get; set; } = "";
		public string Class { get; set; } = "";
		public List<Tab> Tabs { get; set; }	
		public class Tab
		{
			public string Title { get; set; } = "";
			public string Class { get; set; } = "";
			public TabForm Form { get; set; }	
			public class TabForm
			{
				public string TypeForm { get; set; } = "";
				public string FormId { get; set; } = "";
				public string PrimaryKey { get; set; } = "";
				public string Title { get; set; } = "";
				public string? Class { get; set; }	
				public List<Field> Fields { get; set; }	
				public class Field
				{
					public string Key { get; set; } = "";
					public string Label { get; set; } = "";
					public string Type { get; set; } = "";
					public string? Placeholder { get; set; } = "";
					public bool? Required { get; set; }
				}
				public Dictionary<string , object> InitialData { get; set; }
			}
			public TabDetail? Detail { get; set; }
			public class TabDetail
			{
				public string TypeForm { get; set; } = "";
				public string FormId { get; set; } = "";
				public string PrimaryKey { get; set; } = "";
				public string Title { get; set; } = ""; 
				public string Entity { get; set; } = "";
				public string ForeignKey { get; set; } = "";
				public List<FieldDetail> Fields { get; set; }
				public class FieldDetail
				{
					public string Key { get; set; } = "";
					public string Label { get; set; } = "";
					public string Type { get; set; } = "";
					public string? Placeholder { get; set; }
					public List<object>? Options { get; set; }
					public bool? Disabled { get; set; }
					public int? Min { get; set; }
				}
				public List<Dictionary<string , object>> InitialDatas { get; set; }
			}
		}
		public DataProcess? DataProcessing { get; set; }
		public class DataProcess
		{
			public Action Actions { get; set; }
			public class Action
			{
				public List<Post> Posts { get; set; }
				public class Post
				{
					public string Step { get; set; } = "";
					public string Type { get; set; } = "";
					public string Query { get; set; } = "";
				}
			}
		}
	}
}
