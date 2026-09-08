namespace GemBidScraper.DTOs.Python
{
    public class BidDto
    {
        public string BidNumber { get; set; } = "";
        public string BidDate { get; set; } = "";
        public string BidEndDateTime { get; set; } = "";
        public string BidOpeningDateTime { get; set; } = "";
        public int BidValidityDays { get; set; }
        public string TypeOfBid { get; set; } = "";
        public int TotalQuantity { get; set; }
    }
}