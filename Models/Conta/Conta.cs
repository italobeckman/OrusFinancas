using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("Conta")]
    public class Conta
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string NomeBanco { get; set; } = string.Empty;

        public decimal SaldoAtual { get; set; }

        // Conta pertence a um Usuário (1:N)
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Conta tem N Transacoes (1:N)
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}