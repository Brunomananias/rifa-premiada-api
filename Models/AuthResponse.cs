namespace API_Rifa.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }
}
