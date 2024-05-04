using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppBachelorProject.Migrations
{
    public partial class dateCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateCreated",
                table: "Images",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Images");
        }
    }
}
