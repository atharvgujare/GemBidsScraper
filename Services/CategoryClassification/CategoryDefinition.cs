namespace GemBidScraper.Services.CategoryClassification
{
    public class CategoryDefinition
    {
        public string Key { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public List<SubCategory> SubCategories { get; set; } = new();
    }
}