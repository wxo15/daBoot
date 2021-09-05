using Microsoft.EntityFrameworkCore.Migrations;

namespace daBoot.Migrations
{
    public partial class ProfPic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicURL",
                table: "Users",
                type: "text",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicURL",
                table: "Users");
        }
    }
}
