namespace GemBidScraper.DTOs.Python
{
    public class ProductDto
    {
        public List<string> ItemCategory { get; set; } = new();
        public string BOQTitle { get; set; } = "";
        public string PrimaryProductCategory { get; set; } = "";
    }
}