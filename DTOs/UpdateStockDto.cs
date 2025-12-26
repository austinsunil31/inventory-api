namespace Inventory.API.DTOs
{
    public class UpdateStockDto
    {
        public int Id { get; set; }
        public decimal Total_weight { get; set; }
        public int Can_count { get; set; }
    }
}
