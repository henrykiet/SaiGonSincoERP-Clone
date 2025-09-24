using System.Text.Json;
using System.Data;
using System.Data.SqlClient;

namespace Sinco.Server.Models
{
    /// <summary>
    /// Model thông tin form
    /// </summary>
    public class FormInfo
    {
        /// <summary>
        /// Controller xử lý
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Tên bảng
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Khóa chính
        /// </summary>
        public string[] PrimaryKey { get; set; }

        /// <summary>
        /// Loại dữ liệu (list/voucher)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Hành động
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Điều kiện sắp xếp
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// Ngôn ngữ
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Đơn vị
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Mã chứng từ
        /// </summary>
        public string IdVC { get; set; }

        /// <summary>
        /// ID người dùng
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Mở đóng import (FileHandle)
        /// </summary>
        public bool? isFileHandle { get; set; }
    }

    /// <summary>
    /// Model yêu cầu truy vấn dữ liệu động
    /// </summary>
    public class DynamicQueryRequest
    {
        /// <summary>
        /// Thông tin form
        /// </summary>
        public FormInfo FormId { get; set; }

        /// <summary>
        /// Điều kiện lọc dữ liệu
        /// </summary>
        public JsonElement? Filter { get; set; }

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Số bản ghi trên mỗi trang
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Model yêu cầu truy vấn dữ liệu động với filter nâng cao
    /// </summary>
    public class DynamicFilterRequest
    {
        /// <summary>
        /// Thông tin form
        /// </summary>
        public FormInfo FormId { get; set; }

        /// <summary>
        /// Danh sách điều kiện lọc với operator
        /// </summary>
        public List<FilterItem> Filter { get; set; }

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Số bản ghi trên mỗi trang
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Model điều kiện lọc
    /// </summary>
    public class FilterItem
    {
        /// <summary>
        /// Tên trường cần lọc
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Giá trị so sánh
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Toán tử so sánh (=, >=, <=, <, >, like)
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Kiểm tra tính hợp lệ của toán tử
        /// </summary>
        public bool IsValidOperator()
        {
            var validOperators = new[] { "=", ">=", "<=", "<", ">", "like" };
            return validOperators.Contains(Operator?.ToLower());
        }

        /// <summary>
        /// Tạo điều kiện SQL cho filter item
        /// </summary>
        public string ToSqlCondition(string parameterName)
        {
            switch (Operator?.ToLower())
            {
                case "=":
                    return $"{Field} = @{parameterName}";
                case ">=":
                    return $"{Field} >= @{parameterName}";
                case "<=":
                    return $"{Field} <= @{parameterName}";
                case "<":
                    return $"{Field} < @{parameterName}";
                case ">":
                    return $"{Field} > @{parameterName}";
                case "like":
                    return $"{Field} LIKE @{parameterName}";
                default:
                    throw new ArgumentException($"Toán tử '{Operator}' không hợp lệ");
            }
        }

        /// <summary>
        /// Xử lý giá trị cho parameter SQL
        /// </summary>
        public object GetParameterValue()
        {
            if (Operator?.ToLower() == "like")
            {
                // Với LIKE, tự động thêm % nếu chưa có
                if (!Value.Contains('%'))
                {
                    return $"%{Value}%";
                }
            }
            return Value;
        }
    }

    /// <summary>
    /// Model phản hồi kết quả truy vấn dữ liệu động
    /// </summary>
    public class DynamicQueryResponse<T>
    {
        /// <summary>
        /// Controller xử lý
        /// </summary>
        public string Controller { get; set; }

        /// <summary>
        /// Tên bảng
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Khóa chính
        /// </summary>
        public string[] PrimaryKey { get; set; }

        /// <summary>
        /// Ngôn ngữ
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Đơn vị
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Mã chứng từ
        /// </summary>
        public string IdVC { get; set; }

        /// <summary>
        /// Loại dữ liệu (list/voucher)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Hành động
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Điều kiện sắp xếp
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// ID người dùng
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Danh sách dữ liệu
        /// </summary>
        public List<T> Data { get; set; }

        /// <summary>
        /// Tổng số bản ghi
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Số bản ghi trên mỗi trang
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Số bản ghi trên mỗi trang
        /// </summary>
        public bool isFileHandle { get; set; }
    }
} 