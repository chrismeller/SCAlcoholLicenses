using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Data.Models;

namespace SCAlcoholLicenses.Host
{
	class Program
	{
		static void Main(string[] args)
		{
			Run().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		private static async Task Run()
		{
			// midnight, yo
			var seenOn = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

			var client = new LicenseClient();
			using (var db = new ApplicationDbContext())
			{
				var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

				client.GetLicenses((license) =>
				{
					var existing = db.Licenses.FirstOrDefault(x => x.LicenseNumber == license.LicenseNumber && x.OpenDate == license.OpenDate);
					if (existing != null)
					{
						existing.LastSeen = seenOn;
					}
					else
					{

						var dbLicense = new License()
						{
							Id = Guid.NewGuid(),
							BusinessName = license.BusinessName,
							City = license.City,
							CloseDate = license.CloseDate,
							LbdWholesaler = license.LbdWholesaler,
							LegalName = license.LegalName,
							LicenseNumber = license.LicenseNumber,
							LicenseType = license.LicenseType,
							LocationAddress = license.LocationAddress,
							OpenDate = license.OpenDate,

							FirstSeen = seenOn,
							LastSeen = seenOn,
						};

						db.Licenses.Add(dbLicense);
						db.SaveChanges();
					}
				}, () =>
				{
					transaction.Commit();

					transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
				});

			}
		}
	}
}
