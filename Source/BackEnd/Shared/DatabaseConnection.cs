using Microsoft.Extensions.Primitives;

namespace reportSystem01.Shared
{
    public class DatabaseConnection
    {
        public int Id { get; set; }
        public string ConnectionName { get; set; }
        public string DbType { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string SID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public DatabaseConnection()
        {
            // Giá trị mặc định cho SQL Server (có thể chỉnh lại theo nhu cầu)
            ConnectionName = "Default SQL Connection";
            DbType = "SQL Server";  // Các lựa chọn: "SQL Server", "MySQL", "Oracle"
            Server = "localhost";
            Port = 1433; // Port mặc định cho SQL Server
            DatabaseName = "MyDatabase";
            SID = "";  // SID thường không dùng cho SQL Server
            Username = "sa";
            Password = "password";
            IsActive = true;
        }
        // Chuỗi kết nối tự động xây dựng

        public string ConnectionString
        {
            get
            {
                return DbType switch
                {
                    "SQL Server" => $"Server={Server};Initial Catalog={DatabaseName};User Id={Username};Password={Password};TrustServerCertificate=True;",
                    "MySQL" => $"Server={Server};Port={Port};Database={DatabaseName};User={Username};Password={Password};",
                    "Oracle" => $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Server})(PORT={Port}))(CONNECT_DATA=(SERVICE_NAME={DatabaseName})));User Id={Username};Password={Password};",
                    _ => string.Empty
                };
            }
        }
    }
}
