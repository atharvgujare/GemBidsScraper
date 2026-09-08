using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemBidScraper.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BidNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BidDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BidEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BidOpeningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BidValidityDays = table.Column<int>(type: "int", nullable: false),
                    Ministry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Organisation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrievanceEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryProductCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BOQTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    BidType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluationMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InspectionRequired = table.Column<bool>(type: "bit", nullable: false),
                    PaymentTimeline = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EMDRequired = table.Column<bool>(type: "bit", nullable: false),
                    EMDAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    EPBGRequired = table.Column<bool>(type: "bit", nullable: false),
                    EPBGPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    MSEPreference = table.Column<bool>(type: "bit", nullable: false),
                    MIIPreference = table.Column<bool>(type: "bit", nullable: false),
                    ConsigneeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsigneeAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScrapedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bids");
        }
    }
}
