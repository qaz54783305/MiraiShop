using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiraiShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Order",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Order");
        }
    }
}
