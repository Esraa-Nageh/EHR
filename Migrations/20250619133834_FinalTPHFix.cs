using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRsystem.Migrations
{
    /// <inheritdoc />
    public partial class FinalTPHFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Users",
                type: "int",
                nullable: true);
        }
    }
}
