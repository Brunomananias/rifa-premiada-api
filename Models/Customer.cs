using System.ComponentModel.DataAnnotations.Schema;

namespace API_Rifa.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Whatsapp { get; set; } = string.Empty;
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<NumberSold>? NumbersSold { get; set; }
    }
}
