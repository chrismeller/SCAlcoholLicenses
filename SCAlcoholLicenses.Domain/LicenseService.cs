using System;
using System.Data.Entity;
using System.Threading.Tasks;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Data.Models;

namespace SCAlcoholLicenses.Domain
{
	public class LicenseService : IDisposable
	{
		private ApplicationDbContext _db;

		public LicenseService(ApplicationDbContext db)
		{
			_db = db;
		}

		public void Create(string licenseNumber, string businessName, string legalName, string locationAddress, string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, DateTime now)
		{
			var license = new License()
			{
				Id = Guid.NewGuid(),
				LicenseNumber = licenseNumber,
				BusinessName = businessName,
				LegalName = legalName,
				LocationAddress = locationAddress,
				City = city,
				LicenseType = licenseType,
				OpenDate = openDate,
				CloseDate = closeDate,
				LbdWholesaler = lbdWholesaler,
				FirstSeen = now,
				LastSeen = now,
			};

			_db.Licenses.Add(license);
		}

		public async Task<bool> Get(string licenseNumber, DateTime openDate)
		{
			var exists =
				await _db.Licenses.FirstOrDefaultAsync(x => x.LicenseNumber == licenseNumber && x.OpenDate == openDate);

			if (exists == null)
			{
				return false;
			}

			return true;
		}

		public async Task Upsert(string licenseNumber, string businessName, string legalName, string locationAddress,
			string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, DateTime now)
		{
			var existing =
				await _db.Licenses.FirstOrDefaultAsync(x => x.LicenseNumber == licenseNumber && x.OpenDate == openDate);

			if (existing != null)
			{
				existing.LicenseNumber = licenseNumber;
				existing.BusinessName = businessName;
				existing.LegalName = legalName;
				existing.LocationAddress = locationAddress;
				existing.City = city;
				existing.LicenseType = licenseType;
				existing.OpenDate = openDate;
				existing.CloseDate = closeDate;
				existing.LbdWholesaler = lbdWholesaler;
				existing.LastSeen = now;
			}
			else
			{
				Create(licenseNumber, businessName, legalName, locationAddress, city, licenseType, openDate, closeDate, lbdWholesaler, now);
			}

			await _db.SaveChangesAsync();
		}

		public void Dispose()
		{
			_db?.Dispose();
		}
	}
}