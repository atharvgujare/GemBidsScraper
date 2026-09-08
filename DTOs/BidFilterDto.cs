using System.Text.Json.Serialization;

namespace GemBidScraper.DTOs
{
    public class BidFilterDto
    {
        public string? Keyword { get; set; }

        public string? Ministry { get; set; }

        public string? Department { get; set; }

        public string? Organisation { get; set; }

        public string? Office { get; set; }

        public string? Category { get; set; }

        public string? BidType { get; set; }

        // Maps Frontend 'startDate' OR 'fromDate' directly
        [JsonPropertyName("startDate")]
        public DateTime? FromDate { get; set; }

        // Maps Frontend 'endDate' OR 'toDate' directly
        [JsonPropertyName("endDate")]
        public DateTime? ToDate { get; set; }

        public decimal? MinEMD { get; set; }

        public decimal? MaxEMD { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? Status { get; set; }

        public string? SortBy { get; set; }

        public string? CategoryKey { get; set; }

        public string? CategorySubKey { get; set; }
    }
}