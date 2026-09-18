namespace GemBidScraper.Services.CategoryClassification
{
    public class BidClassificationInput
    {
        public string? CardItemName { get; set; }
        public string? ItemCategory { get; set; }
        public string? PrimaryProductCategory { get; set; }
        public string? SimilarCategory { get; set; }
        public string? BOQTitle { get; set; }
        public string? RelevantNotificationCategory { get; set; }
        public string? Specification { get; set; }
    }
}
