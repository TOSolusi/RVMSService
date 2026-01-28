using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class visitmodelupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VehiclePhotoFile",
                table: "Visits",
                newName: "CameraIdName");

            migrationBuilder.RenameColumn(
                name: "VehiclePhoto",
                table: "Visits",
                newName: "CameraIdImage");

            migrationBuilder.RenameColumn(
                name: "VehicleNumber",
                table: "Visits",
                newName: "Camera9Name");

            migrationBuilder.RenameColumn(
                name: "CurrentPhotoFile",
                table: "Visits",
                newName: "Camera8Name");

            migrationBuilder.RenameColumn(
                name: "CurrentPhoto",
                table: "Visits",
                newName: "Camera9Image");

            migrationBuilder.RenameColumn(
                name: "AdditionalPhotoFile",
                table: "Visits",
                newName: "Camera7Name");

            migrationBuilder.RenameColumn(
                name: "AdditionalPhoto",
                table: "Visits",
                newName: "Camera8Image");

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

            migrationBuilder.RenameColumn(
                name: "CameraIdName",
                table: "Visits",
                newName: "VehiclePhotoFile");

            migrationBuilder.RenameColumn(
                name: "CameraIdImage",
                table: "Visits",
                newName: "VehiclePhoto");

            migrationBuilder.RenameColumn(
                name: "Camera9Name",
                table: "Visits",
                newName: "VehicleNumber");

            migrationBuilder.RenameColumn(
                name: "Camera9Image",
                table: "Visits",
                newName: "CurrentPhoto");

            migrationBuilder.RenameColumn(
                name: "Camera8Name",
                table: "Visits",
                newName: "CurrentPhotoFile");

            migrationBuilder.RenameColumn(
                name: "Camera8Image",
                table: "Visits",
                newName: "AdditionalPhoto");

            migrationBuilder.RenameColumn(
                name: "Camera7Name",
                table: "Visits",
                newName: "AdditionalPhotoFile");
        }
    }
}
