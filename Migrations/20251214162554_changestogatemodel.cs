using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class changestogatemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Destinations_DestinationModelDestinationId",
                table: "Gates");

            migrationBuilder.RenameColumn(
                name: "DestinationModelDestinationId",
                table: "Gates",
                newName: "DestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Gates_DestinationModelDestinationId",
                table: "Gates",
                newName: "IX_Gates_DestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Destinations_DestinationId",
                table: "Gates",
                column: "DestinationId",
                principalTable: "Destinations",
                principalColumn: "DestinationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Destinations_DestinationId",
                table: "Gates");

            migrationBuilder.RenameColumn(
                name: "DestinationId",
                table: "Gates",
                newName: "DestinationModelDestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Gates_DestinationId",
                table: "Gates",
                newName: "IX_Gates_DestinationModelDestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Destinations_DestinationModelDestinationId",
                table: "Gates",
                column: "DestinationModelDestinationId",
                principalTable: "Destinations",
                principalColumn: "DestinationId");
        }
    }
}
