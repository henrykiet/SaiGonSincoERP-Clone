using Sinco.Server.Models;
using Sinco.Server.SqlJsonDefinations;
using Sinco.Server.SqlJsonDefinations.Responses;
using System.Data;


namespace Sinco.Server.Repositories.BaseRepository
{
	public interface IBaseRepository<T> where T : class
	{
		Task<List<dynamic>> GetAllAsync(string tableName , int? page , int? pageSize);
		Task<Dictionary<string , List<Dictionary<string , object>>>> GetTablesAsync(Dictionary<string, List<string>> keyValues);
		Task<dynamic?> GetByIdAsync(object id , string tableName , string? keyName);
		Task<int> InsertAsync(dynamic entity , string tableName);
		Task<int> UpdateAsync(T entity , string tableName);
		Task<int> DeleteAsync(string id , string tableName);
        /// <summary>
        /// Delete Multi data
        /// </summary>
        /// <param name="ids">List ID</param>
        /// <param name="tableName">TableName want delete</param>
        /// <param name="status">Status if change</param>
        /// <param name="primaryKey">Primary key table want delete</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        Task<int> DeleteMultiAsync(List<string> ids, string tableName, string status, string primaryKey);
        Task<int> UpsertMultipleTablesAsync(Dictionary<string, DataTable> dataTables, SqlJsonDefination masterDef, List<SqlJsonDefination> foreignDefs, Dictionary<string, object>? userAssign, bool? overWrite);
        Task<int> UpsertAsyncQuaquotationPaper(Dictionary<string, DataTable> dataTables, SqlJsonDefination masterDef, List<SqlJsonDefination> foreignDefs, Dictionary<string, object>? userAssign, bool? overWrite);

		(Dictionary<string, DataTable> tables, List<string> errors) SplitFlatTableWithHeaderDetection(DataTable flatTable, SqlJsonDefination masterDef, List<SqlJsonDefination>? foreignDefs);
		Task<List<(string Code, string? Name)>> GetForeignDataAsync(string sql);

        Task<SqlJsonDefination> GetTemplateByTableNameAsync(string tableName);
		Task<List<SqlJsonDefination>> GetAllTemplateDefinitionsAsync();
		Task<MetadataResponse> GetTemplateMetadataByTableNameAsync(string tableName);
	}
}
