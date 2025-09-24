namespace Sinco.Server.SqlJsonDefinations.Requests
{
	public class MetadataRequest
	{
		/// <summary>
		/// tên bảng
		/// </summary>
		public string FormId { get; set; } = "";
		/// <summary>
		/// Id của khóa chính
		/// </summary>
		public string? Primarykey { get; set; }
		/// <summary>
		/// Định nghĩa cho hành động như insert, update
		/// nếu ko truyền gì mặc định là insert -> form rỗng
		/// </summary>
		public string Action { get; set; } = "";
		///// <summary>
		///// dạng list hay entity
		///// </summary>
		//public string Type { get; set; } = "list";
	}
}
