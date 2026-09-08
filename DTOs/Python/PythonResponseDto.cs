namespace GemBidScraper.DTOs.Python
{
    public class PythonApiResponseDto
    {
        public bool Success { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public PythonApiResponseDto? Data { get; set; }
    }
}