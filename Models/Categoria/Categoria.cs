using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("Categoria")]
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        // Transacao usa a Categoria (1:N)
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

        // Categoria pertence a um Usuário
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}