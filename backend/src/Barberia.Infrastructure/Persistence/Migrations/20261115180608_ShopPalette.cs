using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barberia.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShopPalette : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "palette",
                table: "shops",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Classic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "palette",
                table: "shops");
        }
    }
}
