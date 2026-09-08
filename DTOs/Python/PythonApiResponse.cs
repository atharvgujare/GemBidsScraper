using System.Text.Json;
using System.Text.Json.Serialization;

namespace GemBidScraper.Models
{
    public class PythonApiResponse
    {
        public int Pages { get; set; }

        public int TotalPdfFound { get; set; }

        public int ParsedSuccessfully { get; set; }

        public int Failed { get; set; }

        public List<PythonPdfResult> Result { get; set; } = new();
    }

    public class PythonPdfResult
    {
        [JsonPropertyName("bid_number")]
        public string? BidNumber { get; set; }

        [JsonPropertyName("pdf_url")]
        public string? PdfUrl { get; set; }

        [JsonPropertyName("item_name")]
        public string? ItemName { get; set; }

        [JsonPropertyName("quantity")]
        public string? Quantity { get; set; }

        [JsonPropertyName("ministry")]
        public string? Ministry { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }

        [JsonPropertyName("Data")]
        public JsonElement Data { get; set; }
    }
}