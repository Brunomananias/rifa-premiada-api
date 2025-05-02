namespace API_Rifa.Models
{
    public class GatewaySettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ClientKey { get; set; }
        public string ClientSecret { get; set; }
        public string CompanyId { get; set; }   
    }

}
