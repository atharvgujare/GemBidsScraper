namespace GemBidScraper.DTOs.Python
{
    public class PreferenceDto
    {
        public bool InspectionRequired { get; set; }
        public bool EMDRequired { get; set; }
        public bool EPBGRequired { get; set; }
        public bool MSEPreference { get; set; }
        public bool MIIPreference { get; set; }
        public string EvaluationMethod { get; set; } = "";
        public string PaymentTimeline { get; set; } = "";
    }
}