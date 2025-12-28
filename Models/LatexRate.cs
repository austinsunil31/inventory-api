namespace Inventory.API.Models
{
    public class LatexRate
    {
        public int Id { get; set; }
        public DateTime Rate_Date { get; set; }
        public decimal Latex_Rate { get; set; }
        public DateTime Created_On { get; set; } = DateTime.Now;
    }

}
