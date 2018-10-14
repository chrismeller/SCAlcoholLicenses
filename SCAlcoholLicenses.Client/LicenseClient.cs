using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NLog;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SCAlcoholLicenses.Client
{
	public class LicenseClient
	{
		private readonly ILogger _logger;

		public LicenseClient(ILogger logger)
		{
			_logger = logger;
		}

		public void GetLicenses(Action<AlcoholLicense> recordCallback, Action pageCallback)
		{
			var arguments = ConfigurationManager.AppSettings.Get("Chrome.Arguments");
			var binaryLocation = ConfigurationManager.AppSettings.Get("Chrome.BinaryLocation");

			_logger.Debug("Starting Chrome with arguments: " + arguments);
			_logger.Debug("Starting Chrome with binary location: " + binaryLocation);

			var chromeOptions = new ChromeOptions();

			if (arguments != "") chromeOptions.AddArguments(arguments.Split(','));
			if (binaryLocation != "") chromeOptions.BinaryLocation = binaryLocation;

			var browser = new ChromeDriver(chromeOptions);

			var wait = new WebDriverWait(browser, TimeSpan.FromSeconds(20));

			_logger.Debug("Navigating to MyDORWay");

			browser.Navigate().GoToUrl("https://mydorway.dor.sc.gov/");

			// wait for everything to load - that is, until we can see the link we want to click on
			wait.Until(driver =>
			{
				try
				{
					driver.FindElement(By.XPath(
						"//span[ contains( @class, 'CaptionLinkText' ) and contains( text(), 'Alcohol License Locations' ) ]"));
					return true;
				}
				catch
				{
					return false;
				}
			});

			_logger.Debug("Clicking ABL licenses link");

			// click on the link to get to the ABL licenses page
			browser.FindElementByXPath(
					"//span[ contains( @class, 'CaptionLinkText' ) and contains( text(), 'Alcohol License Locations' ) ]")
				.Click();

			// wait until the page loads and the search button is visible
			wait.Until(driver =>
			{
				try
				{
					driver.FindElement(By.Id("d-9"));
					return true;
				}
				catch
				{
					return false;
				}
			});

			_logger.Debug("Clicking search button");

			// click on the search button
			browser.FindElementById("d-9").Click();

			// wait for results to load
			WaitForResultsToLoad(wait);

			_logger.Debug("Starting loop...");

			var keepGoing = true;
			do
			{
				var licenseNumbers = browser.FindElementsByClassName("d-b");
				var businessNames = browser.FindElementsByClassName("d-c");
				var legalNames = browser.FindElementsByClassName("d-d");
				var locationAddresses = browser.FindElementsByClassName("d-e");
				var cities = browser.FindElementsByClassName("d-f");
				var licenseTypes = browser.FindElementsByClassName("d-g");
				var openDates = browser.FindElementsByClassName("d-h");
				var closeDates = browser.FindElementsByClassName("d-i");
				var lbdWholesalers = browser.FindElementsByXPath("//td[ contains( @class, 'd-j' ) ]//input");

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
				var pageInfo = browser.FindElementById("d-k_pgof").Text.Replace(",", "");

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

					browser.FindElementById("d-k_pgnext").Click();

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
	}
}