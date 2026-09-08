using System.Text.RegularExpressions;

namespace GemBidScraper.Services
{
    public class PdfCleaner
    {
        public string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            text = Regex.Replace(text, @"�+", " ");

            text = Regex.Replace(text, @"[ ]{2,}", " ");

            text = Regex.Replace(text, @"(\r?\n){2,}", "\n");

            text = Regex.Replace(text, @"[\u0900-\u097F]+", "");

            return text.Trim();
        }
    }
}