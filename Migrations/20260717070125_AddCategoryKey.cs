using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemBidScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryKey",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryKey",
                table: "GeMBidExtracts");
        }
    }
}
