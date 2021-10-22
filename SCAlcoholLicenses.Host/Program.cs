using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Domain;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace SCAlcoholLicenses.Host
{
	class Program
	{
		static async Task Main(string[] args)
		{
            var binaryLocation = @"C:\Program Files\Google\Chrome Beta\Application\chrome.exe";
            //var binaryLocation = @"C:\Users\chris\scoop\apps\firefox-developer\current\firefox.exe";

            // midnight, yo
            var seenOn = DateTimeOffset.UtcNow;

			var logger = NLog.LogManager.GetLogger("Default");

			logger.Info("Starting execution.");

			await ParseFile();
			return;

			try
			{
				using var client = new LicenseClient(logger, "", binaryLocation);
				using var db = new ApplicationDbContext();
				using var service = new LicenseService(db);

				var transaction = new TransactionScope();

				await db.GetDbConnection().OpenAsync();

				logger.Info("Getting License file");

				var licenseFilePath = client.GetLicenseFile();

				logger.Info("Parsing License file");
                var recordsUpserted = 0;
                foreach (var license in client.ParseLicenses(licenseFilePath))
                {
                    await Console.Out.WriteLineAsync($"{license.LicenseNumber}");

                    await service.Upsert(license.LicenseNumber, license.BusinessName, license.LegalName,
                        license.LocationAddress, license.City, license.LicenseType, license.OpenDate,
                        license.CloseDate, license.LbdWholesaler, seenOn);

                    recordsUpserted++;

                    if (recordsUpserted % 100 == 0)
                    {
                        transaction.Complete();
						transaction = new TransactionScope();
                    }
                }
            }
			catch (Exception e)
			{
				logger.Error(e, "Execution failed!");
				throw;
			}

			logger.Info("Completed execution.");
		}
		static async Task ParseFile()
        {
            var licenseFilePath = @"C:\Users\chris\Downloads\ABL License Location Query 202110221433.xlsx";

            var logger = NLog.LogManager.GetLogger("Default");
            var seenOn = DateTimeOffset.UtcNow;

			try
			{
				using var client = new LicenseClient(logger, "", null);
				using var db = new ApplicationDbContext();
				using var service = new LicenseService(db);

				var transaction = new TransactionScope();

				await db.GetDbConnection().OpenAsync();

				logger.Info("Getting License file");

				logger.Info("Parsing License file");
				var recordsUpserted = 0;
				foreach (var license in client.ParseLicenses(licenseFilePath))
				{
					//await Console.Out.WriteLineAsync($"{license.LicenseNumber}");

					await service.Upsert(license.LicenseNumber, license.BusinessName, license.LegalName,
						license.LocationAddress, license.City, license.LicenseType, license.OpenDate,
						license.CloseDate, license.LbdWholesaler, seenOn);

					recordsUpserted++;

					if (recordsUpserted % 100 == 0)
					{
						logger.Debug("Completing transaction");

						transaction.Complete();
						transaction = new TransactionScope();
					}
				}

				transaction.Complete();
			}
			catch (Exception e)
			{
				logger.Error(e, "Execution failed!");
				throw;
			}
		}
	}
}
