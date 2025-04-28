namespace API_Rifa.Models
{
    public class Raffle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Total_Numbers { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public string Image_Url { get; set; }
        public string? SoldNumbers { get; set; }
        public int? DrawnNumber { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
