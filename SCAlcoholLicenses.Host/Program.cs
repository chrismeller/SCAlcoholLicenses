using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Data.Models;
using SCAlcoholLicenses.Domain;

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
			var seenOn = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

			var logger = NLog.LogManager.GetLogger("Default");

			logger.Info("Starting execution.");

			try
			{
				var client = new LicenseClient(logger);
				using (var db = new ApplicationDbContext())
				using (var service = new LicenseService(db))
				{
					var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

					client.GetLicenses((license) =>
					{
						var task = service.Upsert(license.LicenseNumber, license.BusinessName, license.LegalName,
							license.LocationAddress, license.City, license.LicenseType, license.OpenDate,
							license.CloseDate, license.LbdWholesaler, seenOn);
						task.Wait();
					}, () =>
					{
						transaction.Commit();

						transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
					});

				}
			}
			catch (Exception e)
			{
				logger.Error(e, "Execution failed!");
			}

			logger.Info("Completed execution.");
		}
	}
}
