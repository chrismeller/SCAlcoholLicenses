using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCAlcoholLicenses.Data.Models
{
	public class License
	{
		public Guid Id { get; set; }

		[Index("UK_LICENSE_OPENDATE", 0, IsUnique = true)]
		[MaxLength(25)]
		public string LicenseNumber { get; set; }
		public string BusinessName { get; set; }
		public string LegalName { get; set; }
		public string LocationAddress { get; set; }
		public string City { get; set; }
		public string LicenseType { get; set; }

		[Index("UK_LICENSE_OPENDATE", 1, IsUnique = true)]
		public DateTime OpenDate { get; set; }
		public DateTime CloseDate { get; set; }
		public bool LbdWholesaler { get; set; }

		public DateTime FirstSeen { get; set; }
		public DateTime LastSeen { get; set; }
	}
}