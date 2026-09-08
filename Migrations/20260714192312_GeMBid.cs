using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GemBidScraper.Migrations
{
    /// <inheritdoc />
    public partial class GeMBid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeMBids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BidNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BidEndDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BidOpeningDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BidValidityDays = table.Column<int>(type: "int", nullable: true),
                    TypeOfBid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BidVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BidCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BidNature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BidTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BidDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BidReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalQuantity = table.Column<int>(type: "int", nullable: true),
                    Ministry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organisation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Office = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerDistrict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerPincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrievanceEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BOQTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryProductCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondaryCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductSubType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrandAllowed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HSNCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SearchKeywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionRequired = table.Column<bool>(type: "bit", nullable: true),
                    EMDRequired = table.Column<bool>(type: "bit", nullable: true),
                    EMDAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EMDPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EPBGRequired = table.Column<bool>(type: "bit", nullable: true),
                    EPBGPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MSEPreference = table.Column<bool>(type: "bit", nullable: true),
                    MIIPreference = table.Column<bool>(type: "bit", nullable: true),
                    StartupPreference = table.Column<bool>(type: "bit", nullable: true),
                    MakeInIndiaPreference = table.Column<bool>(type: "bit", nullable: true),
                    LocalContentPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EvaluationMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTimeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryPincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryDays = table.Column<int>(type: "int", nullable: true),
                    DeliveryPeriod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenderFeeRequired = table.Column<bool>(type: "bit", nullable: true),
                    TenderFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DocumentDownloadStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentDownloadEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClarificationStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClarificationEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreBidMeeting = table.Column<bool>(type: "bit", nullable: true),
                    PreBidMeetingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreBidMeetingVenue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Warranty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Guarantee = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificationRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SampleRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OEMAuthorization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TurnoverCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectionAgency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MSMEAllowed = table.Column<bool>(type: "bit", nullable: true),
                    StartupAllowed = table.Column<bool>(type: "bit", nullable: true),
                    NSICAllowed = table.Column<bool>(type: "bit", nullable: true),
                    GeMSellerRequired = table.Column<bool>(type: "bit", nullable: true),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeItemName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeSerialNo = table.Column<int>(type: "int", nullable: true),
                    ConsigneeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsigneeQuantity = table.Column<int>(type: "int", nullable: true),
                    ConsigneeDeliveryDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeMBids", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeMBids");
        }
    }
}
