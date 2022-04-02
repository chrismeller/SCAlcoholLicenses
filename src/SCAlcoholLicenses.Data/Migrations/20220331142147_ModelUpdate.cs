using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCAlcoholLicenses.Data.Migrations
{
    public partial class ModelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CloseDate",
                table: "Licenses",
                newName: "CloseOrExtensionDate");

            migrationBuilder.AddColumn<bool>(
                name: "FoodProductManufacturer",
                table: "Licenses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FoodProductManufacturer",
                table: "Licenses");

            migrationBuilder.RenameColumn(
                name: "CloseOrExtensionDate",
                table: "Licenses",
                newName: "CloseDate");
        }
    }
}
