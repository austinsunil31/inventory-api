namespace Inventory.API.Models
{
    public class LatexClients
    {
        public int Id { get; set; }
        public string Client_no { get; set; }
        public string Name { get; set; }
        public string Mobile_num { get; set; }
        public string Plot_location { get; set; }
        public DateTime Created_at { get; set; }
        public bool? IsHandledByClient { get; set; }
    }
}
