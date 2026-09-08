namespace GemBidScraper.DTOs.Dashboard;

public class CategoryCardDto
{
    public string Key { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Count { get; set; }

    public string Icon { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;
}