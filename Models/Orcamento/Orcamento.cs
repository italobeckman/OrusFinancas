using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("Orcamento")]
    public class Orcamento
    {
        public int Id { get; set; }
        
        [Required]
        public string Nome { get; set; } = string.Empty;

        public decimal ValorLimite { get; set; }

        // Orcamento pertence a um Usuário (1:N)
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}