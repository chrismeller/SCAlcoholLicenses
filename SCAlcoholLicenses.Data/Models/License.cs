using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace SCAlcoholLicenses.Data.Models
{
    [Index("LicenseNumber", "OpenDate", IsUnique = true)]
	public class License
	{
		public Guid Id { get; set; }

		[MaxLength(25)]
		public string LicenseNumber { get; set; }
		public string BusinessName { get; set; }
		public string LegalName { get; set; }
		public string LocationAddress { get; set; }
		public string City { get; set; }
		public string LicenseType { get; set; }

		public DateTime OpenDate { get; set; }
		public DateTime CloseDate { get; set; }
		public bool LbdWholesaler { get; set; }

		public DateTimeOffset FirstSeen { get; set; }
		public DateTimeOffset LastSeen { get; set; }
	}
}