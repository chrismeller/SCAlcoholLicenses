using Microsoft.EntityFrameworkCore;
using SCAlcoholLicenses.Data.Models;
using System.Data.Common;

namespace SCAlcoholLicenses.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<License> Licenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(
            "Data Source=localhost; Initial Catalog=SCAlcoholLicenses; User Id=SCAlcoholLicenses; Password=SCAlcoholLicenses;");

        public DbConnection GetDbConnection ()
        {
            return this.Database.GetDbConnection();
        }
    }
}