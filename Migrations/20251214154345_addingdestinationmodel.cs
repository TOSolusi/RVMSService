using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class addingdestinationmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gates_Gates_GateModelGateId",
                table: "Gates");

            migrationBuilder.DropIndex(
                name: "IX_Gates_GateModelGateId",
                table: "Gates");

            migrationBuilder.DropColumn(
                name: "GateModelGateId",
                table: "Gates");

            migrationBuilder.AlterColumn<string>(
                name: "GateId",
                table: "Destinations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GateModelGateId",
                table: "Gates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "GateId",
                table: "Destinations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gates_GateModelGateId",
                table: "Gates",
                column: "GateModelGateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gates_Gates_GateModelGateId",
                table: "Gates",
                column: "GateModelGateId",
                principalTable: "Gates",
                principalColumn: "GateId");
        }
    }
}
