using ExcelDataReader;
using Microsoft.VisualBasic.FileIO;
using System.Data;

namespace Sinco.Server.Helpers
{
	public class ReadFileHelper
	{
		public static DataTable ReadCsvToDataTable(IFormFile file)
		{
			var table = new DataTable();

			using var reader = new StreamReader(file.OpenReadStream());
			using var parser = new TextFieldParser(reader)
			{
				TextFieldType = FieldType.Delimited ,
				HasFieldsEnclosedInQuotes = true
			};
			parser.SetDelimiters(",");

			bool isFirstRow = true;
			int columnCount = 0;

			while ( !parser.EndOfData )
			{
				string[] fields = parser.ReadFields() ?? Array.Empty<string>();

				if ( isFirstRow )//header
				{
					foreach ( var header in fields )
						table.Columns.Add(header.Trim().ToLower());
					columnCount = table.Columns.Count;
					isFirstRow = false;
				}
				else//rows
				{
					// Normalize số lượng cột
					var normalized = new string[columnCount];
					Array.Copy(fields , normalized , Math.Min(fields.Length , columnCount));
					//check row null
					if ( normalized.All(string.IsNullOrWhiteSpace) )
						continue;
					table.Rows.Add(normalized);
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
	}
}
