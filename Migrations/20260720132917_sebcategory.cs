using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemBidScraper.Migrations
{
    /// <inheritdoc />
    public partial class sebcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategorySubKey",
                table: "gembidextracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategorySubKey",
                table: "gembidextracts");
        }
    }
}
