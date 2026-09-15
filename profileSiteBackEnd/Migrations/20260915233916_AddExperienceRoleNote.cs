using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace profileSiteBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddExperienceRoleNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoleNote",
                table: "Experiences",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleNote",
                table: "Experiences");
        }
    }
}
