using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SCAlcoholLicenses.Client;
using SCAlcoholLicenses.Domain;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Data.Models;

namespace SCAlcoholLicenses.Host
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _settings;
		private readonly LicenseClient _client;
		private readonly LicenseService _service;
		private readonly IDbConnection _db;

        public App(IOptions<AppSettings> settings, ILogger<App> logger, LicenseClient client, LicenseService service, ApplicationDbContext db)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_service = service ?? throw new ArgumentNullException(nameof(service));
			_db = db.Connection ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task Run()
        {
			try
			{
				_logger.LogDebug("Beginning execution");

                // make sure the db connection is open
                if (_db.State != System.Data.ConnectionState.Open && _db.State != System.Data.ConnectionState.Connecting)
                {
                    _db.Open();
                }

                _logger.LogInformation("Getting License file");
				var licenseFilePath = await _client.GetLicenseFile();

				_logger.LogInformation("Parsing License file");

                var licenses = _client.ParseLicenses(licenseFilePath).ToList();

                _logger.LogInformation($"Found {licenses.Count} licenses.");

				// convert them into database entities
                var dbLicenses = licenses.Select(x => new License()
                {
                    BusinessName = x.BusinessName,
                    City = x.City,
                    CloseOrExtensionDate = x.CloseOrExtensionDate,
                    FoodProductManufacturer = x.FoodProductManufacturer,
                    LbdWholesaler = x.LbdWholesaler,
                    LegalName = x.LegalName,
                    LicenseNumber = x.LicenseNumber,
                    LicenseType = x.LicenseType,
                    LocationAddress = x.LocationAddress,
                    OpenDate = x.OpenDate,

                    // these get overwritten in the service, so they're just placeholders
                    Id = Guid.Empty,
                    FirstSeen = DateTimeOffset.UtcNow,
                    LastSeen = DateTimeOffset.UtcNow,
                });

                await _service.UpsertBatch(dbLicenses);
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
