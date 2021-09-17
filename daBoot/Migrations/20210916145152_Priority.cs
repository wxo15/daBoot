using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace daBoot.Migrations
{
    public partial class Priority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.CreateTable(
                name: "Priority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PriorityName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priority", x => x.Id);
                });
            */
            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PriorityId",
                table: "Tickets",
                column: "PriorityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Priority_PriorityId",
                table: "Tickets",
                column: "PriorityId",
                principalTable: "Priority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Priority_PriorityId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Priority");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PriorityId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "Tickets");
        }
    }
}
