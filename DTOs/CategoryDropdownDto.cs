namespace GemBidScraper.DTOs;

public class CategoryDropdownDto
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int Count { get; set; }

    public List<SubCategoryDropdownDto> SubCategories { get; set; } = new();
}

public class SubCategoryDropdownDto
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int Count { get; set; }
}
