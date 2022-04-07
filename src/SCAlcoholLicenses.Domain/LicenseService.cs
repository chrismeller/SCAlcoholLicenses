using Dapper;
using SCAlcoholLicenses.Data;
using SCAlcoholLicenses.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SCAlcoholLicenses.Domain
{
    public class LicenseService
    {
        private readonly IDbConnection _db;

        public LicenseService(ApplicationDbContext db)
        {
            _db = db.Connection;
        }

        public async Task Create(string licenseNumber, string businessName, string legalName, string locationAddress, string city, string licenseType, DateTime openDate, DateTime closeOrExtensionDate, bool lbdWholesaler, bool foodProductManufacturer, DateTimeOffset now, IDbTransaction transaction)
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
        CloseOrExtensionDate = closeOrExtensionDate,
        LbdWholesaler = lbdWholesaler,
        FoodProductManufacturer = foodProductManufacturer,
        FirstSeen = now,
        LastSeen = now,
    }, transaction);
        }

        public async Task<License> Get(string licenseNumber, string licenseType, DateTime openDate, IDbTransaction transaction)
        {
            var exists = await _db.QueryFirstOrDefaultAsync<License>("select * from Licenses where LicenseNumber = @LicenseNumber and LicenseType = @LicenseType and OpenDate = @OpenDate", new { LicenseNumber = licenseNumber, LicenseType = licenseType, OpenDate = openDate }, transaction);

            return exists;
        }

        public async Task<bool> Exists(string licenseNumber, string licenseType, DateTime openDate, IDbTransaction transaction)
        {
            var existing = await Get(licenseNumber, licenseType, openDate, transaction);
            return existing != null;
        }

        public async Task Upsert(string licenseNumber, string businessName, string legalName, string locationAddress,
            string city, string licenseType, DateTime openDate, DateTime closeDate, bool lbdWholesaler, bool foodProductManufacturer, DateTimeOffset now, IDbTransaction transaction)
        {
            var existing = await Get(licenseNumber, licenseType, openDate, transaction);

            if (existing != null)
            {
                await _db.ExecuteAsync(@"
update Licenses set
	LicenseNumber = @LicenseNumber, BusinessName = @BusinessName, LegalName = @LegalName,
	LocationAddress = @LocationAddress, City = @City, OpenDate = @OpenDate, CloseOrExtensionDate = @CloseOrExtensionDate,
	LbdWholesaler = @LbdWholesaler, FoodProductManufacturer = @FoodProductManufacturer, LastSeen = @LastSeen
where LicenseNumber = @LicenseNumber and LicenseType = @LicenseType and OpenDate = @OpenDate",
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

        public async Task Upsert(License license, DateTimeOffset now, IDbTransaction transaction)
        {
            await Upsert(license.LicenseNumber, license.BusinessName, license.LegalName, license.LocationAddress,
                license.City, license.LicenseType, license.OpenDate, license.CloseOrExtensionDate,
                license.LbdWholesaler, license.FoodProductManufacturer, now, transaction);
        }

        public async Task UpsertBatch(IEnumerable<License> licenses)
        {
            var now = DateTimeOffset.UtcNow;

            using var transaction = _db.BeginTransaction();

            foreach (var license in licenses)
            {
                await Upsert(license, now, transaction);
            }

            transaction.Commit();
        }
    }
}