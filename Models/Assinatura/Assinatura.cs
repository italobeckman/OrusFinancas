using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("Assinatura")]
    public class Assinatura
    {
        public int Id { get; set; }
        
        [Required]
        public string Servico { get; set; } = string.Empty;
        
        public decimal ValorMensal { get; set; }

        // Assinatura pertence a um Usuário (1:N)
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        
        // Assinatura gera N Despesas (1:N)
        // O diagrama sugere que a Despesa é gerada pela Assinatura, mas o relacionamento
        // mais comum é Despesa pertence a Assinatura (N:1) ou Assinatura tem N Despesas.
        // Assumindo Assinatura tem N Despesas:
        public int DespesaId { get; set; }
        public Despesa? Despesa { get; set; } // Aqui seria Despesa Gerada

        public bool Ativa { get; set; }

        public DateTime DataAssinatura { get; set; }
    }
}