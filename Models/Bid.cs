using System.ComponentModel.DataAnnotations;

namespace GemBidScraper.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        // Basic Information
        public string BidNumber { get; set; } = string.Empty;
        public DateTime BidDate { get; set; }

        // Dates
        public DateTime BidEndDate { get; set; }
        public DateTime BidOpeningDate { get; set; }
        public int BidValidityDays { get; set; }

        // Buyer
        public string Ministry { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Organisation { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public string GrievanceEmail { get; set; } = string.Empty;

        // Item
        public string ItemCategory { get; set; } = string.Empty;
        public string PrimaryProductCategory { get; set; } = string.Empty;
        public string BOQTitle { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }

        // Bid Configuration
        public string BidType { get; set; } = string.Empty;
        public string EvaluationMethod { get; set; } = string.Empty;
        public bool InspectionRequired { get; set; }

        public string PaymentTimeline { get; set; } = string.Empty;

        // Financial
        public bool EMDRequired { get; set; }
        public decimal? EMDAmount { get; set; }

        public bool EPBGRequired { get; set; }
        public decimal? EPBGPercentage { get; set; }

        public bool MSEPreference { get; set; }
        public bool MIIPreference { get; set; }

        // Consignee
        public string ConsigneeName { get; set; } = string.Empty;
        public string ConsigneeAddress { get; set; } = string.Empty;
        public int DeliveryDays { get; set; }

        // PDF
        public string PdfUrl { get; set; } = string.Empty;

        // Scraper
        public DateTime ScrapedOn { get; set; } = DateTime.Now;

        public string RawText { get; set; } = string.Empty;
    }
}