using System.ComponentModel.DataAnnotations.Schema;

namespace API_Rifa.Models
{
    public class PixTransaction
    {
        public int Id { get; set; }

        [Column("number_sold_id")]
        public int? Number_Sold_Id { get; set; }

        public NumberSold? Number_Sold { get; set; }

        [Column("pix_key")]
        public string Pix_Key { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("qr_code_url")]
        public string? QrCodeUrl { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }



    public class CheckoutRequest
    {
        public UserDto User { get; set; }
        public int RaffleId { get; set; }
        public List<int> Numbers { get; set; }
        public decimal Price { get; set; }
    }

    public class UserDto
    {
        public string Name { get; set; }
        public string Whatsapp { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public int TransactionId { get; set; }
        public int RaffleId { get; set; }
        public List<int> Numbers { get; set; }
    }


}
