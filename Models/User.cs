using System.ComponentModel.DataAnnotations;

namespace API_Rifa.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = "";

        [MaxLength(20)]
        public string Whatsapp { get; set; } = "";

        [MaxLength(255)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(255)]
        public string Password { get; set; } = "";

        [MaxLength(20)]
        public string Document { get; set; } = "";

        public bool Status { get; set; }
        public bool Is_Admin { get; set; }

        public DateTime Created_At { get; set; } = DateTime.Now;

        public DateTime Updated_At { get; set; } = DateTime.Now;

        public DateTime? Last_Login { get; set; }

        // Relacionamento (ex: numeros vendidos pelo usuário)
        public ICollection<NumberSold> NumbersSold { get; set; }
    }

}
