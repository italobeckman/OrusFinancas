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
        
        // Tipo de categoria - apenas para despesas
        public TipoCategoria TipoCategoria { get; set; } = TipoCategoria.Despesa;

        // Transacao usa a Categoria (1:N) - apenas despesas devem usar
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();

        // Categoria pertence a um Usuário
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
    
    public enum TipoCategoria
    {
        [Display(Name = "Despesa")]
        Despesa = 1,
        
        [Display(Name = "Meta/Orçamento")]
        Meta = 2
    }
}