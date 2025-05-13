using Microsoft.EntityFrameworkCore;
using Backend.Shared.Entities.Models;

namespace Backend.Shared.DataAccess
{
    public class SmartOrdersDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("Server=localhost;Database=smartorders_db;Uid=root;Pwd=357497;");
        }

        public SmartOrdersDbContext(DbContextOptions<SmartOrdersDbContext> options)
        : base(options)
        {
        }
        
        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Website> Websites => Set<Website>();
    }
}