using GemBidScraper.Attributes;
using System.Text.Json.Serialization;

namespace GemBidScraper.Models
{
    public class GeMBidExtract
    {
        public int Id { get; set; }

        public string? BidNumber { get; set; }

        public string? PdfUrl { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        // =========================
        // Bid
        // =========================

        public DateTime? BidEndDateTime { get; set; }

        public DateTime? BidOpeningDateTime { get; set; }

        [JsonField("Bid Offer Validity (From End Date)")]
        public int? BidValidityDays { get; set; }

        public string? TypeOfBid { get; set; }

        public string? EvaluationMethod { get; set; }

        // =========================
        // Card
        // =========================

        public string? CardItemName { get; set; }

        public int? CardQuantity { get; set; }

        public string? CardMinistry { get; set; }

        public string? CardDepartment { get; set; }

        public DateTime? CardStartDate { get; set; }

        public DateTime? CardEndDate { get; set; }

        // =========================
        // Buyer
        // =========================

        [JsonField("Ministry/State Name")]
        public string? Ministry { get; set; }

        public string? DepartmentName { get; set; }

        public string? OrganisationName { get; set; }

        public string? OfficeName { get; set; }

        public string? ContactDetailsOfGrievanceRedressal { get; set; }

        // =========================
        // Product
        // =========================

        public int? TotalQuantity { get; set; }

        public string? ItemCategory { get; set; }

        public string? BOQTitle { get; set; }

        public string? PrimaryProductCategory { get; set; }

        public string? SimilarCategory { get; set; }

        public string? ContractPeriod { get; set; }

        // =========================
        // Eligibility
        // =========================

        public string? MinimumAverageAnnualTurnover { get; set; }

        public string? OEMAverageTurnover { get; set; }

        public string? YearsOfPastExperienceRequired { get; set; }

        public string? PastExperienceRequired { get; set; }

        public string? DocumentRequiredFromSeller { get; set; }

        // =========================
        // Auto Extension
        // =========================

        public int? MinimumNumberOfBidsRequiredToDisableAutomaticBidExtension { get; set; }

        public int? NumberOfDaysForWhichBidWouldBeAutoExtended { get; set; }

        public int? NumberOfAutoExtensionCount { get; set; }

        // =========================
        // Reverse Auction
        // =========================

        public bool? BidToRAEnabled { get; set; }

        public string? RAQualificationRule { get; set; }

        // =========================
        // Inspection
        // =========================

        public bool? InspectionRequired { get; set; }

        public bool? InspectionByBuyerOwnAgency { get; set; }

        public string? InspectionType { get; set; }

        public string? InspectionAgency { get; set; }

        // =========================
        // Financial
        // =========================

        public decimal? EstimatedBidValue { get; set; }

        public decimal? EmdAmount { get; set; }

        public decimal? EPBGPercentage { get; set; }

        public int? EPBGDurationMonths { get; set; }

        public string? AdvisoryBank { get; set; }

        // =========================
        // Preferences
        // =========================

        public bool? MSEPurchasePreference { get; set; }

        public bool? MIIPurchasePreference { get; set; }

        public decimal? PurchasePreferencePercentage { get; set; }

        public decimal? MaximumPurchasePreferencePercentage { get; set; }

        // =========================
        // Legal
        // =========================

        public string? ArbitrationClause { get; set; }

        public string? MediationClause { get; set; }

        // =========================
        // Technical
        // =========================

        public string? Specification { get; set; }

        public string? SpecificationParameterName { get; set; }

        public string? Values { get; set; }

        public string? PhysicalCharacteristics { get; set; }

        public string? Material { get; set; }

        public string? Surface { get; set; }

        public string? Layers { get; set; }

        public string? WarrantyText { get; set; }

        public string? ServiceRequirement { get; set; }

        public string? ServiceInclusions { get; set; }

        public string? TrainingModule { get; set; }

        // =========================
        // GeM Search
        // =========================

        public string? GeMARPTSSearchedStrings { get; set; }

        public string? GeMARPTSSearchedResults { get; set; }

        public string? RelevantCategoriesSelectedForNotification { get; set; }

        // =========================
        // Consignee
        // =========================

        public string? ConsigneeName { get; set; }

        public string? ConsigneeAddress { get; set; }

        public string? ConsigneeQuantity { get; set; }

        // =========================
        // Dynamic Data
        // =========================

        [JsonIgnore]
        public string? TechnicalSpecificationJson { get; set; }

        [JsonIgnore]
        public string? JsonData { get; set; }

        public string? PastPerformance { get; set; }

        public string? PaymentTimelines { get; set; }

        public string? AutoCRACDays { get; set; }

        public string? FinancialDocumentRequired { get; set; }

        public string? Required { get; set; }

        public string? ITCAvailableToBuyer { get; set; }

        public string? MIICompliance { get; set; }

        public string? BuyerSpecificationDocument { get; set; }

        public string? BOQDetailDocument { get; set; }

        [JsonIgnore]
        public string? SpecificationDocument { get; set; }

        public DateTime? BidDate { get; set; }

        public string? CategoryKey { get; set; }


        public string? CategorySubKey { get; set; }

       

    }
}
