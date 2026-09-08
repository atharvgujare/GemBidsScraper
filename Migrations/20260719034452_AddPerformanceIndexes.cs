using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemBidScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrganisationName",
                table: "GeMBidExtracts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ministry",
                table: "GeMBidExtracts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemCategory",
                table: "GeMBidExtracts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentName",
                table: "GeMBidExtracts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryKey",
                table: "GeMBidExtracts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BidNumber",
                table: "GeMBidExtracts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_BidEndDateTime",
                table: "GeMBidExtracts",
                column: "BidEndDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_BidEndDateTime_CreatedOn",
                table: "GeMBidExtracts",
                columns: new[] { "BidEndDateTime", "CreatedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_BidNumber",
                table: "GeMBidExtracts",
                column: "BidNumber",
                unique: true,
                filter: "[BidNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_CategoryKey",
                table: "GeMBidExtracts",
                column: "CategoryKey");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_CreatedOn",
                table: "GeMBidExtracts",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_DepartmentName",
                table: "GeMBidExtracts",
                column: "DepartmentName");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_EmdAmount",
                table: "GeMBidExtracts",
                column: "EmdAmount");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_ItemCategory",
                table: "GeMBidExtracts",
                column: "ItemCategory");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_Ministry",
                table: "GeMBidExtracts",
                column: "Ministry");

            migrationBuilder.CreateIndex(
                name: "IX_GeMBidExtracts_OrganisationName",
                table: "GeMBidExtracts",
                column: "OrganisationName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_BidEndDateTime",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_BidEndDateTime_CreatedOn",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_BidNumber",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_CategoryKey",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_CreatedOn",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_DepartmentName",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_EmdAmount",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_ItemCategory",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_Ministry",
                table: "GeMBidExtracts");

            migrationBuilder.DropIndex(
                name: "IX_GeMBidExtracts_OrganisationName",
                table: "GeMBidExtracts");

            migrationBuilder.AlterColumn<string>(
                name: "OrganisationName",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ministry",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ItemCategory",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentName",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryKey",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BidNumber",
                table: "GeMBidExtracts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
