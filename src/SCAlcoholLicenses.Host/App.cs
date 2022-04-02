using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Domain;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace SCAlcoholLicenses.Host
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _settings;
		private readonly LicenseClient _client;
		private readonly LicenseService _service;
		private readonly DbConnection _db;

        public App(IOptions<AppSettings> settings, ILogger<App> logger, LicenseClient client, LicenseService service, DbConnection db)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_service = service ?? throw new ArgumentNullException(nameof(service));
			_db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task Run()
        {
			try
			{
				_logger.LogDebug("Beginning exection");

				var seenOn = DateTimeOffset.UtcNow;

				// make sure the db connection is open
				if (_db.State != System.Data.ConnectionState.Open && _db.State != System.Data.ConnectionState.Connecting)
				{
					await _db.OpenAsync();
				}

				var transaction = await _db.BeginTransactionAsync();

				_logger.LogInformation("Getting License file");
				var licenseFilePath = await _client.GetLicenseFile();

				_logger.LogInformation("Parsing License file");
				var recordsUpserted = 0;
				foreach (var license in _client.ParseLicenses(licenseFilePath))
				{
					await _service.Upsert(license.LicenseNumber, license.BusinessName, license.LegalName,
						license.LocationAddress, license.City, license.LicenseType, license.OpenDate,
						license.CloseOrExtensionDate, license.LbdWholesaler, license.FoodProductManufacturer, seenOn, transaction);

					recordsUpserted++;

					if (recordsUpserted % 1000 == 0)
					{
						_logger.LogDebug($"Completing transaction. Total records: {recordsUpserted}");

						await transaction.CommitAsync();
						transaction = await _db.BeginTransactionAsync();
					}
				}

				await transaction.CommitAsync();
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Execution failed!");
				throw;
			}

			_logger.LogInformation("Completed execution.");
		}
    }
}
