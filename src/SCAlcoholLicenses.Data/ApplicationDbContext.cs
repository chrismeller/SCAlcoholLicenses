using System.Data;
using Microsoft.EntityFrameworkCore;
using SCAlcoholLicenses.Data.Models;
using System.Data.Common;

namespace SCAlcoholLicenses.Data
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<License> Licenses { get; set; }

        public IDbConnection Connection => Database.GetDbConnection();
    }
}