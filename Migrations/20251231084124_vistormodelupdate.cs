using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class vistormodelupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalPhotoFile",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentPhotoFile",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehiclePhotoFile",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorImageFile",
                table: "Visitors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalPhotoFile",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CurrentPhotoFile",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VehiclePhotoFile",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "VisitorImageFile",
                table: "Visitors");
        }
    }
}
