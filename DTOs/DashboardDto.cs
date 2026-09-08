namespace GemBidScraper.DTOs
{
    public class DashboardDto
    {
        public int TotalBids { get; set; }

        public int ActiveBids { get; set; }

        public int ExpiredBids { get; set; }

        public int Ministries { get; set; }

        public int Departments { get; set; }

        public int Organisations { get; set; }

        public int Categories { get; set; }
    }
}