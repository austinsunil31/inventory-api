namespace Inventory.API.Models.DTOs
{
    public class LatexStockInDto
    {
        public string Client_no { get; set; }
        public decimal Total_weight { get; set; }
        public int Can_count { get; set; }
        public decimal? Sample_Drc { get; set; }
        public bool isHandledByClient { get; set; }
    }
}
