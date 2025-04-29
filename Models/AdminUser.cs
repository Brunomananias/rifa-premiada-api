using System.ComponentModel.DataAnnotations;

namespace API_Rifa.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }

        public bool Status { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }


}
