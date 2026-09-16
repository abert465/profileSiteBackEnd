using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace profileSiteBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityNote",
                table: "Profiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AvailabilityVisible",
                table: "Profiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityNote",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "AvailabilityVisible",
                table: "Profiles");
        }
    }
}
