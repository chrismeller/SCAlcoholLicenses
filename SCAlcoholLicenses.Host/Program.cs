using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Domain;
using System;
using System.Threading.Tasks;

namespace SCAlcoholLicenses.Host
{
	class Program
	{
		static async Task Main(string[] args)
		{
			//var binaryLocation = @"C:\Program Files\Google\Chrome Beta\Application\chrome.exe";
			var binaryLocation = @"C:\Users\chris\scoop\apps\firefox-developer\current\firefox.exe";

			// midnight, yo
			var seenOn = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day);

			var logger = NLog.LogManager.GetLogger("Default");

			logger.Info("Starting execution.");

			try
			{
				using var client = new LicenseClient(logger, "", binaryLocation);
				using var db = new ApplicationDbContext();
				using var service = new LicenseService(db);

				var transaction = await db.Database.BeginTransactionAsync();

				client.GetLicenses((license) =>
				{
					var task = service.Upsert(license.LicenseNumber, license.BusinessName, license.LegalName,
						license.LocationAddress, license.City, license.LicenseType, license.OpenDate,
						license.CloseDate, license.LbdWholesaler, seenOn);
					task.Wait();
				}, () =>
				{
					transaction.Commit();

					transaction = db.Database.BeginTransaction();
				});
			}
			catch (Exception e)
			{
				logger.Error(e, "Execution failed!");
			}

			logger.Info("Completed execution.");
		}
	}
}
