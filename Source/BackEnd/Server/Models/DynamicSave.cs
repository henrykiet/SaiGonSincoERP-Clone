using System.Text.Json;

namespace Sinco.Server.Models
{
    /// <summary>
    /// Model yêu cầu lưu dữ liệu động
    /// </summary>
    public class DynamicSaveRequest
    {
        /// <summary>
        /// Controller xử lý
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// ID của form cần lưu
        /// </summary>
        public string FormId { get; set; }

        /// <summary>
        /// Hành động (insert/update)
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Loại dữ liệu (list/voucher)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ID của người dùng thực hiện
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Đơn vị
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Ngôn ngữ
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Ngày chứng từ
        /// </summary>
        public string VCDate { get; set; }

        /// <summary>
        /// Mã chứng từ
        /// </summary>
        public string IdVC { get; set; }

        /// <summary>
        /// Khóa chính của bảng
        /// </summary>
        public string[] PrimaryKey { get; set; }

        /// <summary>
        /// Dữ liệu cần lưu
        /// </summary>
        public JsonElement Data { get; set; }

        /// <summary>
        /// Dữ liệu file đính kèm
        /// </summary>
        public FileAttachmentRequestData? FileAttachments { get; set; }

        /// <summary>
        /// Xử lý dữ liệu sau khi lưu
        /// </summary>
        public DataProcessing? DataProcessing { get; set; }
    }

    /// <summary>
    /// Model phản hồi kết quả lưu dữ liệu động
    /// </summary>
    public class DynamicSaveResponse
    {
        /// <summary>
        /// Trạng thái thành công hay thất bại
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ID của bản ghi được lưu
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Danh sách lỗi validation
        /// </summary>
        public List<ValidationError> Errors { get; set; }
    }

    /// <summary>
    /// Model thông tin lỗi validation
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Tên trường bị lỗi
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Nội dung lỗi
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Model chi tiết dữ liệu
    /// </summary>
    public class DetailData
    {
        /// <summary>
        /// Controller chi tiết
        /// </summary>
        public string ControllerDetail { get; set; }

        /// <summary>
        /// ID của form chi tiết (tên bảng detail)
        /// </summary>
        public string FormIdDetail { get; set; }

        /// <summary>
        /// Danh sách các trường khóa chính của bảng detail, ngăn cách bằng dấu phẩy
        /// Ví dụ: "ma_kh, line_nbr"
        /// </summary>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Dữ liệu chi tiết dạng mảng các object
        /// </summary>
        public JsonElement Data { get; set; }

        /// <summary>
        /// Lấy danh sách các trường khóa chính của bảng detail
        /// </summary>
        public string[] GetForeignKeyFields()
        {
            return ForeignKey?.Split(',').Select(f => f.Trim()).ToArray() ?? Array.Empty<string>();
        }

        /// <summary>
        /// Lấy danh sách các dòng dữ liệu chi tiết
        /// </summary>
        public IEnumerable<JsonElement> GetDetailRows()
        {
            if (Data.ValueKind == JsonValueKind.Array)
            {
                return Data.EnumerateArray();
            }
            return Enumerable.Empty<JsonElement>();
        }
    }

    /// <summary>
    /// Model yêu cầu xóa dữ liệu động
    /// </summary>
    public class DynamicDeleteRequest
    {
        /// <summary>
        /// Controller xử lý
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// ID của form cần xóa
        /// </summary>
        public string FormId { get; set; }

        /// <summary>
        /// Hành động (delete)
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Loại dữ liệu (list/voucher)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// ID của người dùng thực hiện
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Đơn vị
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Ngôn ngữ
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Ngày chứng từ
        /// </summary>
        public string VCDate { get; set; }

        /// <summary>
        /// Mã chứng từ
        /// </summary>
        public string IdVC { get; set; }

        /// <summary>
        /// Khóa chính của bảng
        /// </summary>
        public string[] PrimaryKey { get; set; }

        /// <summary>
        /// Giá trị cần xóa
        /// </summary>
        public string[] Value { get; set; }

        /// <summary>
        /// Danh sách các bảng cần xóa
        /// </summary>
        public List<string> ListTable { get; set; }
    }

    /// <summary>
    /// Model phản hồi kết quả xóa dữ liệu động
    /// </summary>
    public class DynamicDeleteResponse
    {
        /// <summary>
        /// Trạng thái thành công hay thất bại
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ID của bản ghi đã xóa
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Danh sách lỗi
        /// </summary>
        public List<ValidationError> Errors { get; set; }
    }

    /// <summary>
    /// Model xử lý dữ liệu
    /// </summary>
    public class DataProcessing
    {
        /// <summary>
        /// Các hành động xử lý
        /// </summary>
        public Actions Actions { get; set; }
    }

    /// <summary>
    /// Model các hành động xử lý
    /// </summary>
    public class Actions
    {
        /// <summary>
        /// Các hành động sau khi lưu
        /// </summary>
        public List<ProcessingStep> Post { get; set; }
    }

    /// <summary>
    /// Model bước xử lý
    /// </summary>
    public class ProcessingStep
    {
        /// <summary>
        /// Tên bước
        /// </summary>
        public string Step { get; set; }

        /// <summary>
        /// Loại xử lý
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Câu truy vấn
        /// </summary>
        public string Query { get; set; }
    }

    /// <summary>
    /// Model dữ liệu file đính kèm trong request
    /// </summary>
    public class FileAttachmentRequestData
    {
        /// <summary>
        /// Danh sách files mới cần upload
        /// </summary>
        public List<TempFileData> NewFiles { get; set; } = new List<TempFileData>();

        /// <summary>
        /// Danh sách tên files cần xóa
        /// </summary>
        public List<string> FilesToDelete { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách files đã tồn tại (chỉ để reference)
        /// </summary>
        public List<ExistingFileData> ExistingFiles { get; set; } = new List<ExistingFileData>();
    }

    /// <summary>
    /// Model dữ liệu file tạm thời (từ frontend)
    /// </summary>
    public class TempFileData
    {
        /// <summary>
        /// Tên file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Content type của file
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Dữ liệu file dạng base64
        /// </summary>
        public string FileContent { get; set; }

        /// <summary>
        /// Kích thước file
        /// </summary>
        public int Size { get; set; }
    }

    /// <summary>
    /// Model dữ liệu file đã tồn tại
    /// </summary>
    public class ExistingFileData
    {
        /// <summary>
        /// Tên file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Content type của file
        /// </summary>
        public string ContentType { get; set; }
    }
} 