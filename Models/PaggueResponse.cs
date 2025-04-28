namespace API_Rifa.Models
{
    public class PaggueResponse
    {
        public string Hash { get; set; }
        public string PayerName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string External_Id { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpirationAt { get; set; }
        public string Payment { get; set; }
        public int Status { get; set; } // 0 = pending, 1 = paid
        public string EndToEndId { get; set; }
        public string Reference { get; set; }
        public string QrCodeBase64 { get; set; }
        public string PaymentId { get; set; }
    }
}
