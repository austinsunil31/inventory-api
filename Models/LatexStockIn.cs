namespace Inventory.API.Models.DTOs
{
    public class LatexStockIn
    {
        public int Id { get; set; }
        public string? Client_no { get; set; }
        public decimal Total_weight { get; set; }
        public decimal Latex_weight { get; set; }
        public int Can_count { get; set; }
        public decimal? Sample_drc { get; set; }
        public decimal? Total_drc { get; set; }
        public decimal? Dry_rubber { get; set; }
        public decimal? Dry_rubber_value { get; set; }
        public decimal? Final_value { get; set; }
        public string? Created_by { get; set; }
        public DateTime Created_on { get; set; }
        public bool? Is_drc_added { get; set; }
        public int? processing_fees { get; set; }
    }
}
