using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    // O EF Core suporta Tabela por Hierarquia (TPH) por padrão.
    // Marcar como abstrata exige que Receita e Despesa sejam instanciadas.
    [Table("Transacao")]
    public abstract class Transacao
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime DataTransacao { get; set; }
        
        public decimal Valor { get; set; }
        public string Descricao { get; set; } = string.Empty;

        // Relacionamento com Conta (N:1)
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }

        // Relacionamento com Categoria (N:1)
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int? AssinaturaId { get; set; } 
        public Assinatura? Assinatura { get; set; }
    }
}