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
        [Column("User_Id")]
        public int UserId { get; set; }

        public decimal Value { get; set; }
        [Column("Payment_Status")]
        public string PaymentStatus { get; set; }

        public ICollection<PixTransaction> Pix_Transactions { get; set; }
    }

}
