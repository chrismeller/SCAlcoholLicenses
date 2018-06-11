using System;

namespace SCAlcoholLicenses.Client
{
	public class AlcoholLicense
	{
		public string LicenseNumber { get; set; }
		public string BusinessName { get; set; }
		public string LegalName { get; set; }
		public string LocationAddress { get; set; }
		public string City { get; set; }
		public string LicenseType { get; set; }
		public DateTime OpenDate { get; set; }
		public DateTime CloseDate { get; set; }
		public bool LbdWholesaler { get; set; }
	}
}