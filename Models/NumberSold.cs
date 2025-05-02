using System.ComponentModel.DataAnnotations.Schema;

namespace API_Rifa.Models
{
    public class NumberSold
    {
        public int Id { get; set; }
        [Column("raffle_id")]
        public int RaffleId { get; set; }
        public Raffle Raffle { get; set; }

        public string Numbers { get; set; } // Ex: "001,002,003"
        [Column("customer_id")]
        public int CustomerId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        public decimal Value { get; set; }
        [Column("Payment_Status")]
        public string PaymentStatus { get; set; }

        // Adicione estas propriedades
        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; } // Nullable pois só será preenchido na atualização

        public ICollection<PixTransaction> Pix_Transactions { get; set; }
    }

}
