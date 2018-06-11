using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SCAlcoholLicenses.Client
{
	public class LicenseClient
	{
		public void GetLicenses(Action<AlcoholLicense> recordCallback, Action pageCallback)
		{
			var chromeOptions = new ChromeOptions();
			//chromeOptions.AddArguments("headless");
			chromeOptions.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome Beta\Application\chrome.exe";

			var browser = new ChromeDriver(chromeOptions);

			var wait = new WebDriverWait(browser, TimeSpan.FromSeconds(20));


			browser.Navigate().GoToUrl("https://mydorway.dor.sc.gov/");

			// wait for everything to load
			wait.Until(driver =>
			{
				try
				{
					driver.FindElement(By.Id("caption2_d-a1"));
					return true;
				}
				catch
				{
					return false;
				}
			});

			// click on the link to get to the ABL licenses page
			browser.FindElementById("caption2_d-a1").Click();

			// wait until the page loads
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

			// click on the search button
			browser.FindElementById("d-9").Click();

			// wait for results to load
			WaitForResultsToLoad(wait);

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

				foreach (var license in licenses)
				{
					recordCallback(license);
				}

				pageCallback();

				//var rows = browser.FindElementsByXPath("//table[ @id='d-k' ]/tbody/tr");

				//foreach (var row in rows)
				//{
				//	var licenseNumber = row.FindElement(By.ClassName("d-b")).Text;
				//	var businessName = row.FindElement(By.ClassName("d-c")).Text;
				//	var legalName = row.FindElement(By.ClassName("d-d")).Text;
				//	var locationAddress = row.FindElement(By.ClassName("d-e")).Text;
				//	var city = row.FindElement(By.ClassName("d-f")).Text;
				//	var licenseType = row.FindElement(By.ClassName("d-g")).Text;
				//	var openDate = row.FindElement(By.ClassName("d-h")).Text;
				//	var closeDate = row.FindElement(By.ClassName("d-i")).Text;

				//	var lbdWholesaler = row.FindElement(By.ClassName("d-j")).FindElement(By.TagName("input")).Selected;

				//	var license = new AlcoholLicense()
				//	{
				//		LicenseNumber = licenseNumber,
				//		BusinessName = businessName,
				//		LegalName = legalName,
				//		LocationAddress = locationAddress,
				//		City = city,
				//		LicenseType = licenseType,
				//		OpenDate = DateTime.Parse(openDate),
				//		CloseDate = DateTime.Parse(closeDate),
				//		LbdWholesaler = lbdWholesaler,
				//	};

				//	licenses.Add(license);
				//}

				// get the current page number
				var pageInfo = browser.FindElementById("d-k_pgof").Text.Replace(",", "");

				var currentPage = Convert.ToInt32(pageInfo.Split(new[] {" of "}, StringSplitOptions.None).First());
				var totalPages = Convert.ToInt32(pageInfo.Split(new[] {" of "}, StringSplitOptions.None).Last());
				
				if (currentPage >= totalPages)
				{
					keepGoing = false;
				}
				else
				{
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
					var currentPage = Convert.ToInt32(pageInfo.Split(new[] {" of "}, StringSplitOptions.None).First());

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