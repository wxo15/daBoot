using Microsoft.EntityFrameworkCore.Migrations;

namespace daBoot.Migrations
{
    public partial class ChangeRelationName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relation_Users_TeamMemberId",
                table: "Relation");

            migrationBuilder.DropForeignKey(
                name: "FK_Relation_Users_UserId",
                table: "Relation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Relation",
                table: "Relation");

            migrationBuilder.RenameTable(
                name: "Relation",
                newName: "Relations");

            migrationBuilder.RenameIndex(
                name: "IX_Relation_TeamMemberId",
                table: "Relations",
                newName: "IX_Relations_TeamMemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Relations",
                table: "Relations",
                columns: new[] { "UserId", "TeamMemberId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Relations_Users_TeamMemberId",
                table: "Relations",
                column: "TeamMemberId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Relations_Users_UserId",
                table: "Relations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Relations_Users_TeamMemberId",
                table: "Relations");

            migrationBuilder.DropForeignKey(
                name: "FK_Relations_Users_UserId",
                table: "Relations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Relations",
                table: "Relations");

            migrationBuilder.RenameTable(
                name: "Relations",
                newName: "Relation");

            migrationBuilder.RenameIndex(
                name: "IX_Relations_TeamMemberId",
                table: "Relation",
                newName: "IX_Relation_TeamMemberId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Relation",
                table: "Relation",
                columns: new[] { "UserId", "TeamMemberId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Relation_Users_TeamMemberId",
                table: "Relation",
                column: "TeamMemberId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Relation_Users_UserId",
                table: "Relation",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
