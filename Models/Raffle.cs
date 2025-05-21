using System.ComponentModel.DataAnnotations.Schema;

namespace API_Rifa.Models
{
    public class Raffle
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int? Total_Numbers { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public string Image_Url { get; set; }
        public string? SoldNumbers { get; set; }
        public int? DrawnNumber { get; set; }
        public int User_id { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [Column("winner_id")]
        public int? WinnerId { get; set; }
        [Column("drawn_at")]
        public DateTime? DrawnAt { get; set; }
        public bool? Enabled_Ranking { get; set; }
        public string? Ranking_Message { get; set; }
        public int? Ranking_Quantity { get; set; }
        public string? Type_Ranking { get; set; }
        public string? WinnerName { get; set; }
        public string? WinnerPhone { get; set; }

        public ICollection<NumberSold>? NumbersSold { get; set; }
    }

}
