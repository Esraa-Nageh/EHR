using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRsystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSpecializationAndAddRemainingDoctorPatientFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "DoctorRatings",
                type: "double",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "DoctorRatings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);
        }
    }
}
