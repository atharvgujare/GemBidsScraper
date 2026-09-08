namespace GemBidScraper.DTOs
{
    public class BidCardDto
    {
        public string BidNumber { get; set; } = "";
        public string RANumber { get; set; } = "";
        public string Item { get; set; } = "";
        public int Quantity { get; set; }

        public string Ministry { get; set; } = "";
        public string Department { get; set; } = "";

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string PdfUrl { get; set; } = "";
    }
}