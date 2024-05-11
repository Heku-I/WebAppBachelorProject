using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppBachelorProject.Migrations
{
    public partial class updt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Evaluation",
                table: "Images",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Evaluation",
                table: "Images");
        }
    }
}
