using Microsoft.EntityFrameworkCore;
using CEMS.Models;

namespace CEMS.Data
{
    //(IConfiguration configuration) is a constructor parameter to give access to configuration settings, like appsettings.json
    //DbContext is an EF Core class that handles databse connection, queries and saving data
    public class ApplicationDbContext(IConfiguration configuration) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Running base class's method first
            base.OnConfiguring(optionsBuilder);
            
            //Reading from appsettings.json
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        }
        
        //Stating the formation of so-and-so database tables.
        public DbSet<User> Users { get; set; }
    }
}