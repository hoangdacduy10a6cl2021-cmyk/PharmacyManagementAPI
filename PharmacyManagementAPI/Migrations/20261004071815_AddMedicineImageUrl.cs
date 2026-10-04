using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicineImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Medicines",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Medicines");
        }
    }
}
