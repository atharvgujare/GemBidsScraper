using GemBidScraper.DTOs;
using Microsoft.Playwright;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GemBidScraper.Services
{
    public class BrowserService
    {
        public async Task<List<BidCardDto>> GetBidCardsAsync(int totalPages = 30)
        {
            var result = new List<BidCardDto>();

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false
                });

            var page = await browser.NewPageAsync();

            await page.GotoAsync("https://bidplus.gem.gov.in/all-bids");

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var nextButton = await page.QuerySelectorAsync("text=Next");

            Console.WriteLine(nextButton == null
                ? "Next Button Not Found"
                : "Next Button Found");

            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                Console.WriteLine($"========== PAGE {currentPage} ==========");

                var cards = await page.QuerySelectorAllAsync("div.card");

                Console.WriteLine($"Cards Found : {cards.Count}");

                foreach (var card in cards)
                {
                    BidCardDto dto = new();

                //--------------------------------------------------
                // Bid Number + PDF
                //--------------------------------------------------

                var bidLink = await card.QuerySelectorAsync("a.bid_no_hover");

                if (bidLink != null)
                {
                    dto.BidNumber = (await bidLink.InnerTextAsync()).Trim();

                    var href = await bidLink.GetAttributeAsync("href");

                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        dto.PdfUrl = new Uri(
                            new Uri("https://bidplus.gem.gov.in"),
                            href).ToString();
                    }
                }

                //--------------------------------------------------
                // RA Number
                //--------------------------------------------------

                var header = await card.QuerySelectorAsync("p.bid_no");

                if (header != null)
                {
                    string headerText = await header.InnerTextAsync();

                    Console.WriteLine(headerText);

                    var ra = Regex.Match(headerText, @"GEM/\d{4}/R/\d+");

                    if (ra.Success)
                        dto.RANumber = ra.Value;
                }

                //--------------------------------------------------
                // Quantity
                //--------------------------------------------------

                string cardText = await card.InnerTextAsync();

                var qty = Regex.Match(cardText, @"Quantity\s*:?\s*(\d+)");

                if (qty.Success)
                    dto.Quantity = int.Parse(qty.Groups[1].Value);

                //--------------------------------------------------
                // Item
                //--------------------------------------------------

                var itemAnchor = await card.QuerySelectorAsync("a[data-content]");

                if (itemAnchor != null)
                {
                    dto.Item =
                        await itemAnchor.GetAttributeAsync("data-content") ?? "";

                    if (string.IsNullOrWhiteSpace(dto.Item))
                    {
                        dto.Item = (await itemAnchor.InnerTextAsync()).Trim();
                    }
                }

                //--------------------------------------------------
                // Department Block
                //--------------------------------------------------

                var deptBlock = await card.QuerySelectorAsync("div.col-md-5");

                if (deptBlock != null)
                {
                    var lines = (await deptBlock.InnerTextAsync())
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();

                    Console.WriteLine("-------------");
                    foreach (var l in lines)
                        Console.WriteLine(l);

                    if (lines.Count >= 2)
                        dto.Ministry = lines[1];

                    if (lines.Count >= 3)
                        dto.Department = lines[2];
                }

                //--------------------------------------------------
                // Date Block
                //--------------------------------------------------

                var dateBlock = await card.QuerySelectorAsync("div.col-md-3");

                if (dateBlock != null)
                {
                    string dateText = await dateBlock.InnerTextAsync();

                    Console.WriteLine(dateText);

                    var start = Regex.Match(
                        dateText,
                        @"Start Date:\s*(.*?)\s*End Date:",
                        RegexOptions.Singleline);

                    if (start.Success)
                    {
                        if (TryParseGemDate(start.Groups[1].Value.Trim(), out DateTime s))
                            dto.StartDate = s;
                    }

                    var end = Regex.Match(
                        dateText,
                        @"End Date:\s*(.*)",
                        RegexOptions.Singleline);

                    if (end.Success)
                    {
                        if (TryParseGemDate(end.Groups[1].Value.Trim(), out DateTime e))
                            dto.EndDate = e;
                    }
                }

                    if (!result.Any(x => x.BidNumber == dto.BidNumber))
                    {
                        result.Add(dto);
                    }

                }

                if (currentPage == totalPages)
                    break;

                Console.WriteLine($"Moving to Page {currentPage + 1}");

                var firstBid = await page.Locator("a.bid_no_hover").First.InnerTextAsync();

                var nextLocator = page.Locator("text=Next");

                if (await nextLocator.CountAsync() == 0)
                {
                    Console.WriteLine("Next button not found.");
                    break;
                }

                await nextLocator.ScrollIntoViewIfNeededAsync();

                await nextLocator.ClickAsync();
                // Wait until the first bid changes
                await page.WaitForFunctionAsync(
                    @"oldBid => {
                    const el = document.querySelector('a.bid_no_hover');
                    return el && el.innerText !== oldBid;
                }",
                                firstBid);

                await page.WaitForTimeoutAsync(1000);
            }
            Console.WriteLine($"Total Bids Scraped: {result.Count}");
            await browser.CloseAsync();

            return result;
        }

        private static bool TryParseGemDate(string value, out DateTime date)
        {
            value = value.Trim();

            string[] formats =
 {
    "dd-MM-yyyy",
    "dd-MM-yyyy HH:mm",
    "dd-MM-yyyy HH:mm:ss",
    "dd-MM-yyyy hh:mm tt",
    "dd-MM-yyyy hh:mm:ss tt",

    "dd-MMM-yyyy hh:mm tt",
    "dd-MMM-yyyy HH:mm",

    "dd/MM/yyyy",
    "dd/MM/yyyy hh:mm tt",
    "dd/MM/yyyy HH:mm"
};

            return DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }
    }
}