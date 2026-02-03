using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class updatevisitmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CapturedImageDataModel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CameraName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CapturedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CameraType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisitModelVisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapturedImageDataModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapturedImageDataModel_Visits_VisitModelVisitId",
                        column: x => x.VisitModelVisitId,
                        principalTable: "Visits",
                        principalColumn: "VisitId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapturedImageDataModel_VisitModelVisitId",
                table: "CapturedImageDataModel",
                column: "VisitModelVisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapturedImageDataModel");
        }
    }
}
