using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVMSService.Migrations
{
    /// <inheritdoc />
    public partial class addinggatemodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GateModelGateId",
                table: "Gates",
                type: "uniqueidentifier",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
