using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farmora.Infastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStampVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecurityStampVersion",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "bytea",
                nullable: false,
                defaultValue: Array.Empty<byte>());
        }
    }
}
