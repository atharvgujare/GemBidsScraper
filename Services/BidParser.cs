using System.Text.RegularExpressions;

namespace GemBidScraper.Services
{
    public class BidParser
    {
        public object Parse(string text)
        {
            return new
            {
                BidNumber = GetValue(text, @"Bid Number\s*(GEM/\d{4}/B/\d+)"),

                BidDate = GetValue(text, @"Dated:\s*([0-9\-]+)"),

                BidEndDate = GetValue(text,
                    @"Bid End Date/Time\s*([0-9:\-\s]+)"),

                BidOpeningDate = GetValue(text,
                    @"Bid Opening.*?([0-9:\-\s]+)"),

                Ministry = GetValue(text,
                    @"Ministry/State Name\s*(.+)"),

                Department = GetValue(text,
                    @"Department Name\s*(.+)"),

                Organisation = GetValue(text,
                    @"Organisation Name\s*(.+)"),

                Office = GetValue(text,
                    @"Office Name\s*(.+)"),

                Quantity = GetValue(text,
                    @"Total Quantity\s*(\d+)"),

                ItemCategory = GetValue(text,
                    @"Item Category\s*(.+)")
            };
        }

        private string GetValue(string text, string pattern)
        {
            var match = Regex.Match(
                text,
                pattern,
                RegexOptions.Singleline);

            if (match.Success)
                return match.Groups[1].Value.Trim();

            return "";
        }
    }
}