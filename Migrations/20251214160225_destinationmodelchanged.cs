using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class destinationmodelchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GateId",
                table: "Destinations");

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationModelDestinationId",
                table: "Gates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gates_DestinationModelDestinationId",
                table: "Gates",
                column: "DestinationModelDestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Destinations_DestinationModelDestinationId",
                table: "Gates",
                column: "DestinationModelDestinationId",
                principalTable: "Destinations",
                principalColumn: "DestinationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Destinations_DestinationModelDestinationId",
                table: "Gates");

            migrationBuilder.DropIndex(
                name: "IX_Gates_DestinationModelDestinationId",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "DestinationModelDestinationId",
                table: "Gates");

            migrationBuilder.AddColumn<string>(
                name: "GateId",
                table: "Destinations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
