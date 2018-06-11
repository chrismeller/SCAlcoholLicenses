using System.Data.Entity;
using SCAlcoholLicenses.Data.Models;

namespace SCAlcoholLicenses.Data
{
	public class ApplicationDbContext : DbContext
	{
		public virtual DbSet<License> Licenses { get; set; }
	}
}