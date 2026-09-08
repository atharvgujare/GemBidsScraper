using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemBidScraper.Migrations
{
    /// <inheritdoc />
    public partial class newScrapper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeMBidExtracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BidNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BidEndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BidOpeningDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BidValidityDays = table.Column<int>(type: "int", nullable: true),
                    TypeOfBid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaluationMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ministry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganisationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactDetailsOfGrievanceRedressal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalQuantity = table.Column<int>(type: "int", nullable: true),
                    ItemCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BOQTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryProductCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SimilarCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumAverageAnnualTurnover = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OEMAverageTurnover = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearsOfPastExperienceRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PastExperienceRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentRequiredFromSeller = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinimumNumberOfBidsRequiredToDisableAutomaticBidExtension = table.Column<int>(type: "int", nullable: true),
                    NumberOfDaysForWhichBidWouldBeAutoExtended = table.Column<int>(type: "int", nullable: true),
                    NumberOfAutoExtensionCount = table.Column<int>(type: "int", nullable: true),
                    BidToRAEnabled = table.Column<bool>(type: "bit", nullable: true),
                    RAQualificationRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionRequired = table.Column<bool>(type: "bit", nullable: true),
                    InspectionByBuyerOwnAgency = table.Column<bool>(type: "bit", nullable: true),
                    InspectionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionAgency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedBidValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EmdAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EPBGPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EPBGDurationMonths = table.Column<int>(type: "int", nullable: true),
                    AdvisoryBank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MSEPurchasePreference = table.Column<bool>(type: "bit", nullable: true),
                    MIIPurchasePreference = table.Column<bool>(type: "bit", nullable: true),
                    PurchasePreferencePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumPurchasePreferencePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ArbitrationClause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediationClause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationParameterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhysicalCharacteristics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Surface = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Layers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WarrantyText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceRequirement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceInclusions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrainingModule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeMARPTSSearchedStrings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeMARPTSSearchedResults = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelevantCategoriesSelectedForNotification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeQuantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TechnicalSpecificationJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PastPerformance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTimelines = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AutoCRACDays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinancialDocumentRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Required = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ITCAvailableToBuyer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MIICompliance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerSpecificationDocument = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BOQDetailDocument = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificationDocument = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeMBidExtracts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeMBidExtracts");
        }
    }
}
