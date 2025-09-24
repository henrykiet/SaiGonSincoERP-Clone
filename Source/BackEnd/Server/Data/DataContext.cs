using Microsoft.EntityFrameworkCore;
using reportSystem01.Shared;
    
namespace reportSystem01.Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) 
        { 
        
        }

        public virtual DbSet<User> Users { get; set; }
    }
}
