using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace SCAlcoholLicenses.Client
{
    public class LicenseClient
    {
        private readonly ILogger _logger;
        private readonly string _proxyHostname;
        private readonly string _proxyUsername;
        private readonly string _proxyPassword;

        public LicenseClient(ILogger<LicenseClient> logger, string proxyHostname = null, string proxyUsername = null, string proxyPassword = null)
        {
            _logger = logger;
            _proxyHostname = proxyHostname;
            _proxyUsername = proxyUsername;
            _proxyPassword = proxyPassword;
        }

        public async Task<string> GetLicenseFile()
        {
            var browserOptions = new BrowserTypeLaunchOptions();

            if (!String.IsNullOrWhiteSpace(_proxyHostname))
            {
                _logger.LogDebug($"Using proxy server {_proxyHostname}");

                browserOptions.Proxy = new Proxy
                {
                    Server = _proxyHostname,
                    Username = _proxyUsername,
                    Password = _proxyPassword,
                };
            }

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(browserOptions);

            _logger.LogDebug("Navigating to MyDORWay...");
            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://mydorway.dor.sc.gov");

            _logger.LogDebug("Clicking license locations link...");
            await page.Locator("//span[ contains( text(), 'Alcohol License Locations' ) ]").ClickAsync();

            _logger.LogDebug("Clicking search button...");
            await page.Locator("//span[ contains( text(), 'SEARCH' ) ]/ancestor::button").ClickAsync();

            _logger.LogDebug("Clicking export link...");
            var waitForDownloadTask = page.WaitForDownloadAsync();
            await page.Locator("//span[ contains( text(), 'Export Data' ) ]/ancestor::li").ClickAsync();

            var download = await waitForDownloadTask;
            var path = await download.PathAsync();

            if (path == null) throw new Exception("Download file does not exist.");

            // we copy the file out because playwright cleans up when the browser exits
            var tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            File.Move(path, tempPath);

            return tempPath;
        }

        public IEnumerable<AlcoholLicense> ParseLicenses(string licenseFilePath)
        {
            _logger.LogInformation("Parsing licenses...");

            using var stream = File.OpenRead(licenseFilePath);
            using var reader = ExcelReaderFactory.CreateReader(stream);

            do
            {
                while (reader.Read())
                {
                    // if this is the header row, just skip ahead
                    if (reader.GetString(0) == "License Number") continue;

                    var license = new AlcoholLicense
                    {
                        LicenseNumber = reader.GetString(0),
                        BusinessName = reader.GetString(1),
                        LegalName = reader.GetString(2),
                        LocationAddress = reader.GetString(3),
                        City = reader.GetString(4),
                        LicenseType = reader.GetString(5),
                        OpenDate = reader.GetDateTime(6),
                        CloseOrExtensionDate = reader.GetDateTime(7),
                        LbdWholesaler = reader.GetBoolean(8),
                        FoodProductManufacturer = reader.GetBoolean(9),
                    };

                    yield return license;
                }
            } while (reader.NextResult());
        }
    }
}