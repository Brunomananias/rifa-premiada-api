namespace API_Rifa.Models
{
    public class PixConfig
    {
        public int Id { get; set; }
        public string PixKey { get; set; }
        public string PixCopiaCola { get; set; } // <-- nova coluna para o Pix Copia e Cola
    }

    public class RifaPixAssociation
    {
        public int RaffleId { get; set; }
        public Raffle Rifa { get; set; }

        public int PixConfigId { get; set; }
        public PixConfig PixConfig { get; set; }
    }

}
