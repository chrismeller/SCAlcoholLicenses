using Dapper;
using SCAlcoholLicenses.Data.Models;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace SCAlcoholLicenses.Domain
{
    public class LicenseService
	{
		private DbConnection _db;

		public LicenseService(DbConnection db)
		{
			_db = db;
		}

		public async Task Create(string licenseNumber, string businessName, string legalName, string locationAddress, string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, bool foodProductManufacturer, DateTimeOffset now, DbTransaction transaction)
		{
			await _db.ExecuteAsync(@"
insert into Licenses
	(Id, LicenseNumber, BusinessName, LegalName, LocationAddress, City, LicenseType, OpenDate, CloseOrExtensionDate, LbdWholesaler, FoodProductManufacturer, FirstSeen, LastSeen)
values
	(@Id, @LicenseNumber, @BusinessName, @LegalName, @LocationAddress, @City, @LicenseType, @OpenDate, @CloseOrExtensionDate, @LbdWholesaler, @FoodProductManufacturer, @FirstSeen, @LastSeen)",
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
		CloseOrExtensionDate = closeDate,
		LbdWholesaler = lbdWholesaler,
		FoodProductManufacturer = foodProductManufacturer,
		FirstSeen = now,
		LastSeen = now,
	}, transaction);
		}

		public async Task<License> Get(string licenseNumber, string licenseType, DateTime openDate, DbTransaction transaction)
		{
			var exists = await _db.QueryFirstOrDefaultAsync<License>("select * from Licenses where LicenseNumber = @LicenseNumber and LicenseType = @LicenseType and OpenDate = @OpenDate", new { LicenseNumber = licenseNumber, LicenseType = licenseType, OpenDate = openDate }, transaction);

			return exists;
		}

		public async Task<bool> Exists(string licenseNumber, string licenseType, DateTime openDate, DbTransaction transaction) {
			var existing = await Get(licenseNumber, licenseType, openDate, transaction);
			return existing != null;
		}

		public async Task Upsert(string licenseNumber, string businessName, string legalName, string locationAddress,
			string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, bool foodProductManufacturer, DateTimeOffset now, DbTransaction transaction)
		{
			var existing = await Get(licenseNumber, licenseType, openDate, transaction);

			if (existing != null)
			{
				await _db.ExecuteAsync(@"
update Licenses set
	LicenseNumber = @LicenseNumber, BusinessName = @BusinessName, LegalName = @LegalName,
	LocationAddress = @LocationAddress, City = @City, LicenseType = @LicenseType, OpenDate = @OpenDate, CloseOrExtensionDate = @CloseOrExtensionDate,
	LbdWholesaler = @LbdWholesaler, FoodProductManufacturer = @FoodProductManufacturer, LastSeen = @LastSeen
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
						CloseOrExtensionDate = closeDate,
						LbdWholesaler = lbdWholesaler,
						FoodProductManufacturer = foodProductManufacturer,
						LastSeen = now,
                    }, transaction);
			}
			else
			{
				await Create(licenseNumber, businessName, legalName, locationAddress, city, licenseType, openDate, closeDate, lbdWholesaler, foodProductManufacturer, now, transaction);
			}
		}
	}
}