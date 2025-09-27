using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace OrusFinancas.Models
{
    [Table("Conta")]
    public class Conta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(50)]
        public string NomeBanco { get; set; } = string.Empty; // Nome ou Instituição (ex: NuBank, Banco do Brasil)

        [Required(ErrorMessage = "O saldo inicial é obrigatório.")]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoInicial { get; set; } // O saldo com que a conta foi aberta

        // Campo para o Requisito: Identifica o tipo de conta (Corrente, Cartão, Carteira)
        [Required(ErrorMessage = "O tipo da conta é obrigatório.")]
        public TipoConta Tipo { get; set; } 

        // O SaldoAtual deve ser calculado: SaldoInicial + (Total Receitas) - (Total Despesas)
        // Por isso, ele não deve ser persistido.
        [NotMapped] // <-- Use [NotMapped] para um campo que é calculado e não vai para o DB
        public decimal SaldoAtual { get; set; } 

        // Relacionamento com Usuário (1:N)
        [Required] // Toda Conta deve ter um Usuário
        public int UsuarioId { get; set; } // Mudança: Deve ser int (obrigatório)
        public Usuario Usuario { get; set; } = default!; // Mudança: Deve ser não-nullable se a FK for obrigatória

        // Propriedades de Navegação
        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
        public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
    }

    // Enum para o Requisito: Tipos de Conta
    public enum TipoConta
    {
        [Display(Name = "Conta Corrente/Poupança")]
        Corrente = 1,
        
        [Display(Name = "Cartão de Crédito")]
        CartaoCredito = 2,
        
        [Display(Name = "Carteira Digital")]
        CarteiraDigital = 3
    }
}