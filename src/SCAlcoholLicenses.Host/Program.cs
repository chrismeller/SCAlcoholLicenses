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

            var seenOn = DateTimeOffset.UtcNow;

			var logger = NLog.LogManager.GetLogger("Default");

			logger.Info("Starting execution.");

			try
			{
				using var client = new LicenseClient(logger, "", null);
				using var db = new ApplicationDbContext();
				var service = new LicenseService(db);

				await db.GetDbConnection().OpenAsync();

				var transaction = await db.GetDbConnection().BeginTransactionAsync();

				logger.Info("Getting License file");
				var licenseFilePath = client.GetLicenseFile();

				logger.Info("Parsing License file");
				var recordsUpserted = 0;
				foreach (var license in client.ParseLicenses(licenseFilePath))
				{
					await service.Upsert(license.LicenseNumber, license.BusinessName, license.LegalName,
						license.LocationAddress, license.City, license.LicenseType, license.OpenDate,
						license.CloseDate, license.LbdWholesaler, seenOn, transaction);

					recordsUpserted++;

					if (recordsUpserted % 1000 == 0)
					{
						logger.Debug($"Completing transaction. Total records: {recordsUpserted}");

						await transaction.CommitAsync();
						transaction = await db.GetDbConnection().BeginTransactionAsync();
					}
				}

				await transaction.CommitAsync();
			}
			catch (Exception e)
			{
				logger.Error(e, "Execution failed!");
				throw;
			}

			logger.Info("Completed execution.");
		}
	}
}
