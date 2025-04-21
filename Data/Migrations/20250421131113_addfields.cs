using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSFiberCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class addfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "A1s",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "As",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Bf",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Bft",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Bft3",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "My",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "N",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Qx",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "a1_cm",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "a_cm",
                table: "Fiber",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "A1s",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "As",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "Bf",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "Bft",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "Bft3",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "My",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "N",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "Qx",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "a1_cm",
                table: "Fiber");

            migrationBuilder.DropColumn(
                name: "a_cm",
                table: "Fiber");
        }
    }
}
