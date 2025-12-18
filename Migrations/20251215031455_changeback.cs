using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class changeback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Destinations_DestinationId",
                table: "Gates");

            migrationBuilder.DropIndex(
                name: "IX_Gates_DestinationId",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "DestinationId",
                table: "Gates");

            migrationBuilder.AddColumn<string>(
                name: "Gates",
                table: "Destinations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gates",
                table: "Destinations");

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationId",
                table: "Gates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gates_DestinationId",
                table: "Gates",
                column: "DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Destinations_DestinationId",
                table: "Gates",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "DestinationId");
        }
    }
}
