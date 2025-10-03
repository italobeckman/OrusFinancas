using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    /// <summary>
    /// Tabela de junção para o relacionamento Many-to-Many entre Transacao e Tag.
    /// Esta é a implementação explícita do relacionamento N:N.
    /// </summary>
    [Table("TransacaoTag")]
    public class TransacaoTag
    {
        // Chaves Estrangeiras Compostas (Primary Key)
        public int TransacaoId { get; set; }
        public Transacao Transacao { get; set; } = default!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = default!;

        // Propriedade opcional: Data em que a tag foi adicionada à transação
        public DateTime DataAssociacao { get; set; } = DateTime.Now;
    }
}
