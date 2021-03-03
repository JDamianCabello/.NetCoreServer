using Microsoft.EntityFrameworkCore;
using SharedNameSpace;

namespace CallCenterServer
{
    /// <summary>
    /// Represents a combination of the Unit Of Work and Repository patterns such that it can be used to query from a database and group together changes that will then be written back to the store as a unit.
    /// </summary>
    public class SQLiteDBContext : DbContext
    {
        /// <summary>
        /// Represents an entity set that can be used for create, read, update, and delete operations. 
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Create the Database config, here is the database name
        /// </summary>
        /// <param name="options"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=CallCenterUsers.db"); //Database name here
    }
}
