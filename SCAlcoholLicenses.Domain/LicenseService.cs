using Dapper;
using Microsoft.EntityFrameworkCore.Storage;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Data.Models;
using System;
using System.Data.Common;
using System.Data.Entity;
using System.Threading.Tasks;

namespace SCAlcoholLicenses.Domain
{
    public class LicenseService : IDisposable
	{
		private DbConnection _db;

		public LicenseService(ApplicationDbContext db)
		{
			_db = db.GetDbConnection();
		}

		public async Task Create(string licenseNumber, string businessName, string legalName, string locationAddress, string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, DateTimeOffset now)
		{
			await _db.ExecuteAsync(@"
insert into Licenses
	(Id, LicenseNumber, BusinessName, LegalName, LocationAddress, City, LicenseType, OpenDate, CloseDate, LbdWholesaler, FirstSeen, LastSeen)
values
	(@Id, @LicenseNumber, @BusinessName, @LegalName, @LocationAddress, @City, @LicenseType, @OpenDate, @CloseDate, @LbdWholesaler, @FirstSeen, @LastSeen)",
	new
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
	});
		}

		public async Task<License> Get(string licenseNumber, DateTime openDate)
		{
			var exists = await _db.QueryFirstOrDefaultAsync<License>("select * from Licenses where LicenseNumber = @LicenseNumber and OpenDate = @OpenDate", new { LicenseNumber = licenseNumber, OpenDate = openDate });

			return exists;
		}

		public async Task<bool> Exists(string licenseNumber, DateTime openDate) {
			var existing = await Get(licenseNumber, openDate);
			return existing != null;
		}

		public async Task Upsert(string licenseNumber, string businessName, string legalName, string locationAddress,
			string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, DateTimeOffset now)
		{
			var existing = await Get(licenseNumber, openDate);

			if (existing != null)
			{
				await _db.ExecuteAsync(@"
update Licenses set
	LicenseNumber = @LicenseNumber, BusinessName = @BusinessName, LegalName = @LegalName,
	LocationAddress = @LocationAddress, City = @City, LicenseType = @LicenseType, OpenDate = @OpenDate, CloseDate = @CloseDate,
	LbdWholesaler = @LbdWholesaler, LastSeen = @LastSeen
where LicenseNumber = @LicenseNumber and OpenDate = @OpenDate",
					new
                    {
						LicenseNumber = licenseNumber,
						BusinessName = businessName,
						LegalName = legalName,
						LocationAddress = locationAddress,
						City = city,
						LicenseType = licenseType,
						OpenDate = openDate,
						CloseDate = closeDate,
						LbdWholesaler = lbdWholesaler,
						LastSeen = now,
                    });
			}
			else
			{
				await Create(licenseNumber, businessName, legalName, locationAddress, city, licenseType, openDate, closeDate, lbdWholesaler, now);
			}
		}

		public void Dispose()
		{
			_db?.Dispose();
		}
	}
}