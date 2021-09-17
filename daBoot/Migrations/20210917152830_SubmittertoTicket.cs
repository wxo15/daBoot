using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace daBoot.Migrations
{
    public partial class SubmittertoTicket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDateTime",
                table: "Tickets",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmitterId",
                table: "Tickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SubmitterId",
                table: "Tickets",
                column: "SubmitterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_SubmitterId",
                table: "Tickets",
                column: "SubmitterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_SubmitterId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SubmitterId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SubmittedDateTime",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SubmitterId",
                table: "Tickets");
        }
    }
}
