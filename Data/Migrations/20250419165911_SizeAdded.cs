using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSFiberCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class SizeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Length",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Length",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Fiber");
        }
    }
}
