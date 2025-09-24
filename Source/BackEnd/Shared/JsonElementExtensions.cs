using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace reportSystem01.Shared
{
    public static class JsonElementExtensions
    {
        public static object GetValue(this JsonElement element, string type)
        {
            try
            {
                switch (type.ToLower())
                {
                    case "date":
                        if (element.ValueKind == JsonValueKind.String && DateTime.TryParse(element.GetString(), out DateTime dateValue))
                            return dateValue.ToString("yyyy-MM-dd"); // Định dạng ngày theo yêu cầu
                        return DBNull.Value;

                    case "int":
                        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int intValue))
                            return intValue;
                        return DBNull.Value;

                    case "numeric":
                        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out decimal decimalValue))
                            return decimalValue;
                        return DBNull.Value;

                    default:
                        return element.ValueKind switch
                        {
                            JsonValueKind.String => element.GetString(),
                            JsonValueKind.Number => element.GetDecimal(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => DBNull.Value,
                            _ => element.ToString()
                        };
                }
            }
            catch
            {
                return DBNull.Value;
            }
        }

        public static int ExtractIntValue(object obj)
        {
            if (obj is JsonElement jsonElement)
            {
                switch (jsonElement.ValueKind)
                {
                    case JsonValueKind.Number:
                        if (jsonElement.TryGetInt32(out int intValue))
                            return intValue;
                        if (jsonElement.TryGetInt64(out long longValue))
                            return (int)longValue;
                        break;
                    case JsonValueKind.String:
                        if (int.TryParse(jsonElement.GetString(), out int parsed))
                            return parsed;
                        break;
                }
            }
            else if (obj is int intVal)
            {
                return intVal;
            }
            else if (obj is long longVal)
            {
                return (int)longVal;
            }
            else if (obj is string strVal && int.TryParse(strVal, out int parsedStr))
            {
                return parsedStr;
            }

            // Giá trị không hợp lệ, trả về một giá trị mặc định hoặc ném ngoại lệ
            return 1; // Ví dụ: trang hiện tại mặc định là 1
        }
    }

}
