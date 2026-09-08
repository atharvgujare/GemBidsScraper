namespace GemBidScraper.DTOs.Python
{
    public class ConsigneeDto
    {
        public string Item { get; set; } = "";
        public int SerialNo { get; set; }
        public string Consignee { get; set; } = "";
        public string Address { get; set; } = "";
        public int Quantity { get; set; }
        public int DeliveryDays { get; set; }
    }
}