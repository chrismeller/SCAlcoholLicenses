using ExcelDataReader;
using NLog;
using OfficeOpenXml;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SCAlcoholLicenses.Client
{
    public class LicenseClient : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _remoteDriverUri;
        private readonly string _chromeArgs;
        private readonly string _binaryLocation;

        private IWebDriver _browser { get; set; }

        public LicenseClient(ILogger logger, string chromeArgs, string binaryLocation)
        {
            _logger = logger;

            _chromeArgs = chromeArgs;
            _binaryLocation = binaryLocation;
        }

        public string GetLicenseFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            //_logger.Debug($"Starting Chrome with arguments: {_chromeArgs}");
            //_logger.Debug($"Starting Chrome with binary location: {_binaryLocation}");
            _logger.Debug($"Using download directory: {tempDir}");

            var chromeOptions = new ChromeOptions();
            //chromeOptions.AddUserProfilePreference("download.default_directory", tempDir);

            //if (!string.IsNullOrEmpty(_chromeArgs)) chromeOptions.AddArguments(_chromeArgs.Split(','));
            //if (!string.IsNullOrEmpty(_binaryLocation)) chromeOptions.BinaryLocation = _binaryLocation;

            //_browser = new ChromeDriver(chromeOptions);
            _browser = new RemoteWebDriver(new Uri("http://localhost:4444"), chromeOptions);

            var wait = new WebDriverWait(_browser, TimeSpan.FromSeconds(20));

            _logger.Debug("Navigating to MyDORWay");

            _browser.Navigate().GoToUrl("https://mydorway.dor.sc.gov");

            // wait until the page has loaded - that is, until we can see the link we want to click on, then click on it
            _logger.Debug("Clicking on Alcohol Licenses link");
            wait.Until(b => b.FindElement(By.XPath("//span[ contains( text(), 'Alcohol License Locations' ) ]"))).Click();

            // wait for the search button to be visible and click it to submit the form
            _logger.Debug("Clicking on Search button");
            wait.Until(b => b.FindElement(By.XPath("//span[ contains( text(), 'SEARCH' ) ]/ancestor::button"))).Click();

            // set up our file watcher so we can tell when the download is complete
            _logger.Debug("Starting file watcher");
            var watcher = new FileSystemWatcher(tempDir, "*.xlsx");

            // wait for the export data button to be visible and click it
            _logger.Debug("Clicking Export link");
            wait.Until(b => {
                var el = b.FindElement(By.XPath("//span[ contains( text(), 'Export Data' ) ]/ancestor::li"));
                if (el.GetCssValue("display") != "none")
                {
                    return el;
                }

                return null;
            }).Click();

            // just wait until the watcher triggers
            var changedFile = watcher.WaitForChanged(WatcherChangeTypes.Changed);

            return Path.Combine(tempDir, changedFile.Name);

        }

        public IEnumerable<AlcoholLicense> ParseLicenses(string licenseFilePath)
        {
            // we loop and wait for an exclusive read lock to make sure the file is done writing
            _logger.Debug("Waiting for file lock");
            using var stream = WaitForLock(licenseFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
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
                        CloseDate = reader.GetDateTime(7),
                        LbdWholesaler = reader.GetBoolean(8),
                    };

                    yield return license;
                }
            } while (reader.NextResult());
        }

        private FileStream WaitForLock(string path, FileMode mode, FileAccess access, FileShare share)
        {
            for(var tries = 0; tries < 100; tries++)
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(path, mode, access, share);
                    return stream;
                }
                catch (Exception)
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                    Thread.Sleep(100);
                }
            }

            return null;
        }

        public void Dispose()
        {
            _browser?.Quit();
            _browser?.Dispose();
        }
    }
}