using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium.Firefox;

namespace SCAlcoholLicenses.Client
{
    public class LicenseClient : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _chromeArgs;
        private readonly string _binaryLocation;

        private IWebDriver _browser { get; set; }

        public LicenseClient(ILogger logger, string chromeArgs, string binaryLocation)
        {
            _logger = logger;

            _chromeArgs = chromeArgs;
            _binaryLocation = binaryLocation;
        }

        public void GetLicenses(Action<AlcoholLicense> recordCallback, Action pageCallback)
        {
            _logger.Debug("Starting Chrome with arguments: " + _chromeArgs);
            _logger.Debug("Starting Chrome with binary location: " + _binaryLocation);

            //var chromeOptions = new ChromeOptions
            //{
            //    Proxy = new Proxy()
            //    {
            //        //SocksProxy = "socks5://us9333.nordvpn.com",
            //        //SocksProxy = "us9235.nordvpn.com",
            //        SocksProxy = "localhost:1080",
            //        //SocksUserName = "tYoFPfS7JjADyCMnLy65RCoq",
            //        //SocksPassword = "6CWdFxx4gaDwiq6oxdFz7jN6",
            //        SocksVersion = 5,
            //        //SslProxy = "tYoFPfS7JjADyCMnLy65RCoq:6CWdFxx4gaDwiq6oxdFz7jN6@us9235.nordvpn.com",
            //    }
            //};

            var chromeOptions = new FirefoxOptions
            {
                //Proxy = new Proxy
                //{
                //    //SocksProxy = "us.socks.nordhold.net",
                //    SocksProxy = "localhost:1080",
                //    //SocksUserName = "tYoFPfS7JjADyCMnLy65RCoq",
                //    //SocksPassword = "6CWdFxx4gaDwiq6oxdFz7jN6",
                //    SocksVersion = 5,
                //},
            };

            if (_chromeArgs != "") chromeOptions.AddArguments(_chromeArgs.Split(','));
            //if (_binaryLocation != "") chromeOptions.BinaryLocation = _binaryLocation;
            if (_binaryLocation != "") chromeOptions.BrowserExecutableLocation = _binaryLocation;

            //_browser = new ChromeDriver(chromeOptions);
            _browser = new FirefoxDriver(chromeOptions);

            var wait = new WebDriverWait(_browser, TimeSpan.FromSeconds(20));

            _logger.Debug("Navigating to MyDORWay");

            _browser.Navigate().GoToUrl("https://mydorway.dor.sc.gov/");

            // wait for everything to load - that is, until we can see the link we want to click on
            wait.Until(driver =>
            {
                try
                {
                    driver.FindElement(By.XPath(
                        "//span[ contains( text(), 'Alcohol License Locations' ) ]"));
                    return true;
                }
                catch
                {
                    return false;
                }
            });

            _logger.Debug("Clicking ABL licenses link");

            // click on the link to get to the ABL licenses page
            _browser.FindElement(By.XPath(
                    "//span[ contains( text(), 'Alcohol License Locations' ) ]"))
                .Click();

            // wait until the page loads and the search button is visible
            wait.Until(driver =>
            {
                try
                {
                    driver.FindElement(By.XPath("//span[ contains( text(), 'SEARCH' ) ]/ancestor::button"));
                    return true;
                }
                catch
                {
                    return false;
                }
            });

            _logger.Debug("Clicking search button");

            // click on the search button
            _browser.FindElement(By.XPath("//span[ contains( text(), 'SEARCH' ) ]/ancestor::button")).Click();

            // wait for results to load
            WaitForResultsToLoad(wait);

            _logger.Debug("Starting loop...");

            var keepGoing = true;
            do
            {
                var licenseNumbers = _browser.FindElements(By.ClassName("d-b"));
                var businessNames = _browser.FindElements(By.ClassName("d-c"));
                var legalNames = _browser.FindElements(By.ClassName("d-d"));
                var locationAddresses = _browser.FindElements(By.ClassName("d-e"));
                var cities = _browser.FindElements(By.ClassName("d-f"));
                var licenseTypes = _browser.FindElements(By.ClassName("d-g"));
                var openDates = _browser.FindElements(By.ClassName("d-h"));
                var closeDates = _browser.FindElements(By.ClassName("d-i"));
                var lbdWholesalers = _browser.FindElements(By.XPath("//td[ contains( @class, 'd-j' ) ]//input"));

                _logger.Debug("Got " + licenseNumbers.Count + " records");

                var licenses = new List<AlcoholLicense>();
                for (var i = 0; i < licenseNumbers.Count; i++)
                {
                    var licenseNumber = licenseNumbers[i];
                    var businessName = businessNames[i];
                    var legalName = legalNames[i];
                    var locationAddress = locationAddresses[i];
                    var city = cities[i];
                    var licenseType = licenseTypes[i];
                    var openDate = openDates[i];
                    var closeDate = closeDates[i];
                    var lbdWholesaler = lbdWholesalers[i];

                    var license = new AlcoholLicense()
                    {
                        LicenseNumber = licenseNumber.Text,
                        BusinessName = businessName.Text,
                        LegalName = legalName.Text,
                        LocationAddress = locationAddress.Text,
                        City = city.Text,
                        LicenseType = licenseType.Text,
                        OpenDate = DateTime.Parse(openDate.Text),
                        CloseDate = DateTime.Parse(closeDate.Text),
                        LbdWholesaler = lbdWholesaler.Selected,
                    };

                    licenses.Add(license);
                }

                _logger.Debug("Parsing complete");

                foreach (var license in licenses)
                {
                    recordCallback(license);
                }

                pageCallback();

                _logger.Debug("Page complete");

                // get the current page number
                var pageInfo = _browser.FindElement(By.Id("d-k_pgof")).Text.Replace(",", "");

                var currentPage = Convert.ToInt32(pageInfo.Split(new[] { " of " }, StringSplitOptions.None).First());
                var totalPages = Convert.ToInt32(pageInfo.Split(new[] { " of " }, StringSplitOptions.None).Last());

                if (currentPage >= totalPages)
                {
                    _logger.Debug("Current page " + currentPage + " is >= than " + totalPages + " total pages, stopping.");

                    keepGoing = false;
                }
                else
                {
                    _logger.Debug("Navigating to page " + (currentPage + 1));

                    _browser.FindElement(By.Id("d-k_pgnext")).Click();

                    WaitForResultsToLoad(wait, currentPage + 1);
                }

            } while (keepGoing);
        }

        private void WaitForResultsToLoad(WebDriverWait wait, int page = 1)
        {
            wait.Until(driver =>
            {
                try
                {
                    var spinner = driver.FindElement(By.Id("FastBusySpinner"));

                    if (spinner.Displayed)
                    {
                        return false;
                    }

                    var pageInfo = driver.FindElement(By.Id("d-k_pgof")).Text.Replace(",", "");
                    var currentPage = Convert.ToInt32(pageInfo.Split(new[] { " of " }, StringSplitOptions.None).First());

                    if (currentPage < page)
                    {
                        return false;
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public void Dispose()
        {
            _browser?.Dispose();
        }
    }
}