namespace API_Rifa.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string Billing_Cycle { get; set; } = ""; // Ex: "mensal", "único"
        public int Campaign_Limit { get; set; }
        public int Number_Limit { get; set; }
        public bool Is_Active { get; set; }
        public DateTime Created_At { get; set; }
    }
}
