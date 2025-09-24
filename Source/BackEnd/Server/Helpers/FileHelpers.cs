using ExcelDataReader;
using Microsoft.VisualBasic.FileIO;
using Sinco.Server.SqlJsonDefinations;
using System.Data;
using static Sinco.Server.SqlJsonDefinations.SqlJsonDefination.ExcelIntegrationMap;

namespace Sinco.Server.Helpers
{
    public class FileHelpers
    {
        public static DataTable ReadCsvToDataTableWithMapping(
                                                            IFormFile file,
                                                            List<SqlJsonDefination.ExcelIntegrationMap.ExcelColumnMapping> columnMappings,
                                                            bool hasHeader = true)
        {
            var table = new DataTable();
            var headerIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StreamReader(file.OpenReadStream());
            using var parser = new TextFieldParser(reader)
            {
                TextFieldType = FieldType.Delimited,
                HasFieldsEnclosedInQuotes = true
            };
            parser.SetDelimiters(",");

            bool isFirstRow = true;

            while (!parser.EndOfData)
            {
                string[] fields = parser.ReadFields() ?? Array.Empty<string>();

                if (isFirstRow)
                {
                    if (hasHeader)
                    {
                        // Xử lý header
                        for (int i = 0; i < fields.Length; i++)
                        {
                            var excelColumn = fields[i].Trim();
                            headerIndexMap[excelColumn] = i;

                            // Xử lý trùng tên cột
                            var colName = excelColumn;
                            int duplicateCount = 1;
                            while (table.Columns.Contains(colName))
                            {
                                colName = $"{excelColumn}{duplicateCount++}";
                            }

                            var mapping = columnMappings.FirstOrDefault(x =>
                                excelColumn.Contains(x.ExcelColumn, StringComparison.OrdinalIgnoreCase));

                            if (mapping != null)
                            {
                                var mapColName = mapping.FieldName;
                                duplicateCount = 1;
                                while (table.Columns.Contains(mapColName))
                                {
                                    mapColName = $"{mapping.FieldName}{duplicateCount++}";
                                }
                                table.Columns.Add(mapColName);
                            }
                            else
                            {
                                table.Columns.Add(excelColumn + $"__{i}");
                            }
                        }
                    }
                    else
                    {
                        // Không có header → tạo cột từ mapping hoặc tên mặc định
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (i < columnMappings.Count)
                                table.Columns.Add(columnMappings[i].FieldName);
                            else
                                table.Columns.Add($"Column{i}");
                        }

                        // Thêm dòng đầu tiên như dữ liệu
                        var row = table.NewRow();
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            row[i] = i < fields.Length ? fields[i] : DBNull.Value;
                        }
                        table.Rows.Add(row);
                    }

                    isFirstRow = false;
                }
                else
                {
                    // Giữ nguyên cả dòng trống → không continue
                    var row = table.NewRow();

                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (i < fields.Length)
                        {
                            var value = fields[i];

                            // Lấy mapping theo index (nếu có)
                            var mapping = i < columnMappings.Count ? columnMappings[i] : null;

                            if (string.IsNullOrWhiteSpace(value))
                            {
                                if (mapping?.Type?.Equals("number", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    row[i] = 0; // số → để 0
                                }
                                else
                                {
                                    row[i] = DBNull.Value; // text → null
                                }
                            }
                            else
                            {
                                if (mapping?.Type?.Equals("number", StringComparison.OrdinalIgnoreCase) == true
                                    && double.TryParse(value, out var num))
                                {
                                    row[i] = num; // convert về số
                                }
                                else
                                {
                                    row[i] = value;
                                }
                            }
                        }
                        else
                        {
                            row[i] = DBNull.Value;
                        }
                    }

                    table.Rows.Add(row);
                }
            }

            return table;
        }

        public static DataTable ReadXlsxToDataTable(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });
            return dataSet.Tables[0];
        }

        public static DataTable ConvertToDataTable(List<Dictionary<string, object>> data)
        {
            var dt = new DataTable();

            if (data == null || data.Count == 0)
                return dt;

            // 1. Thu thập tất cả các cột có thể có từ mọi dòng
            var allKeys = new HashSet<string>();
            foreach (var dict in data)
            {
                foreach (var key in dict.Keys)
                {
                    allKeys.Add(key);
                }
            }

            // 2. Thêm cột vào DataTable
            foreach (var key in allKeys)
            {
                dt.Columns.Add(key);
            }

            // 3. Thêm từng dòng dữ liệu
            foreach (var dict in data)
            {
                var row = dt.NewRow();
                foreach (var key in dict.Keys)
                {
                    row[key] = dict[key] ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }

            return dt;
        }
        public static DataTable ConvertToDataTable(List<Dictionary<string, object>> data, string tableName, int index)
        {
            var dt = new DataTable();
            dt.TableName = $"{tableName}{index}";

            if (data == null || data.Count == 0)
                return dt;

            // 1. Thu thập tất cả các cột
            var allKeys = new HashSet<string>();
            foreach (var dict in data)
            {
                foreach (var key in dict.Keys)
                {
                    allKeys.Add(key);
                }
            }

            // 2. Thêm cột vào DataTable (chỉ thêm index 1 lần ở đây)
            foreach (var key in allKeys)
            {
                dt.Columns.Add($"{key}{index}");
            }

            // 3. Thêm từng dòng dữ liệu
            foreach (var dict in data)
            {
                var row = dt.NewRow();
                foreach (var key in dict.Keys)
                {
                    var columnName = $"{key}{index}";
                    if (dt.Columns.Contains(columnName))
                    {
                        row[columnName] = dict[key] ?? DBNull.Value;
                    }
                }
                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}
