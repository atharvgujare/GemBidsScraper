namespace GemBidScraper.Services.CategoryClassification
{
    public class SubCategory
    {
        public string Key { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public List<string> Keywords { get; set; } = new();
    }
}