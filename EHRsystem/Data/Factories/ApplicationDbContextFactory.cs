using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EHRsystem.Data;
using EHRsystem.Models.Entities;

namespace EHRsystem.Factories
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(
                "server=localhost;port=3306;database=UnifiedEHRSystemDB;user=root;",
                new MySqlServerVersion(new Version(8, 0, 36))
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
