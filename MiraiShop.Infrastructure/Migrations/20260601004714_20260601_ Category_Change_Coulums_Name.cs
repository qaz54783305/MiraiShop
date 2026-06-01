using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiraiShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260601_Category_Change_Coulums_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Category",
                newName: "CategoryName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "Category",
                newName: "Name");
        }
    }
}
