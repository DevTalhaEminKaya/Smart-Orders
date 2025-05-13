using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace Backend.Shared.DataAccess
{
    public class SmartOrdersDbContextFactory : IDesignTimeDbContextFactory<SmartOrdersDbContext>
    {
        public SmartOrdersDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SmartOrdersDbContext>();

            optionsBuilder.UseMySQL(
                "Server=localhost;Database=smartorders_db;Uid=root;Pwd=357497;"
            );

            return new SmartOrdersDbContext(optionsBuilder.Options);
        }
    }
}