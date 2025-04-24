namespace API_Rifa.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Whatsapp { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
        public DateTime Updated_At { get; set; } = DateTime.Now;

        public ICollection<NumberSold> NumbersSold { get; set; }
    }

}
