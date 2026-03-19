using Microsoft.EntityFrameworkCore;
using CEMS.Models;

namespace CEMS.Data
{

    public class ApplicationDbContext(IConfiguration configuration) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                .UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        }
        
        //Stating the formation of so-and-so database tables.
        public DbSet<User> Users { get; set; }
    }
}