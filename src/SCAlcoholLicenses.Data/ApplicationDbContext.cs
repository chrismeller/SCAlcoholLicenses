using Microsoft.EntityFrameworkCore;
using SCAlcoholLicenses.Data.Models;
using System.Data.Common;

namespace SCAlcoholLicenses.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<License> Licenses { get; set; }

        public DbConnection GetDbConnection ()
        {
            return this.Database.GetDbConnection();
        }
    }
}