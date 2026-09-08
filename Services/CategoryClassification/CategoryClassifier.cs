using GemBidScraper.Services.CategoryClassification;

public class CategoryClassifier
{
    private readonly List<CategoryDefinition> _categories;

    public CategoryClassifier()
    {
        _categories = CategoryKeywordProvider.Categories;
    }

    public CategoryResult Classify(string? title, string? description)
    {
        string text = $"{title} {description}".ToLowerInvariant();

        foreach (var category in _categories)
        {
            foreach (var subCategory in category.SubCategories)
            {
                foreach (var keyword in subCategory.Keywords)
                {
                    if (text.Contains(keyword.ToLowerInvariant()))
                    {
                        return new CategoryResult
                        {
                            CategoryKey = category.Key,
                            CategorySubKey = subCategory.Key
                        };
                    }
                }
            }
        }

        return new CategoryResult
        {
            CategoryKey = "OTHER",
            CategorySubKey = "OTHER"
        };
    }
}