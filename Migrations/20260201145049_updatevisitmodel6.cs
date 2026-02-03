using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class updatevisitmodel6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameraIdImage",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CameraIdName",
                table: "Visits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "CameraIdImage",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CameraIdName",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
