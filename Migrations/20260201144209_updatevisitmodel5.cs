using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class updatevisitmodel5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VisitorImageFile",
                table: "Visitors",
                newName: "VisitorImageName");

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera10Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera10Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera1Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera1Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera2Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera2Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera3Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera3Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera4Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera4Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera5Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera5Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera6Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera6Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera7Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera7Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera8Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera8Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Camera9Image",
                table: "Visits",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Camera9Name",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Camera10Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera10Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera1Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera1Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera2Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera2Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera3Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera3Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera4Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera4Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera5Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera5Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera6Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera6Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera7Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera7Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera8Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera8Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera9Image",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Camera9Name",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CameraIdImage",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CameraIdName",
                table: "Visits");

            migrationBuilder.RenameColumn(
                name: "VisitorImageName",
                table: "Visitors",
                newName: "VisitorImageFile");
        }
    }
}
