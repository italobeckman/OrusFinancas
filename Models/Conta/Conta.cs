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
        [Display(Name = "Banco/Instituição")]
        public Bancos NomeBanco { get; set; }

        /// <summary>
        /// Saldo com que a conta foi aberta no sistema.
        /// Este valor é usado apenas como referência histórica e NÃO deve ser alterado após a criação.
        /// Serve para calcular a variação financeira desde a criação da conta.
        /// </summary>
        [Required(ErrorMessage = "O saldo inicial é obrigatório.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Saldo ao Cadastrar")]
        public decimal SaldoInicial { get; set; }

        [Required(ErrorMessage = "O tipo da conta é obrigatório.")]
        [Display(Name = "Tipo de Conta")]
        public TipoConta Tipo { get; set; }

        /// <summary>
        /// Saldo atual calculado dinamicamente: Soma de TODAS as transações (Receitas - Despesas).
        /// NÃO usa o SaldoInicial no cálculo, apenas as transações registradas no sistema.
        /// </summary>
        [NotMapped]
        [Display(Name = "Saldo Atual")]
        public decimal SaldoAtual { get; set; }

        /// <summary>
        /// Variação total desde a criação da conta (SaldoAtual - SaldoInicial).
        /// Mostra o quanto a conta cresceu ou diminuiu desde que foi cadastrada.
        /// </summary>
        [NotMapped]
        [Display(Name = "Variação Total")]
        public decimal VariacaoTotal => SaldoAtual - SaldoInicial;

        /// <summary>
        /// Indica se a conta teve evolução positiva desde a criação
        /// </summary>
        [NotMapped]
        public bool TemCrescimento => VariacaoTotal > 0;

        // Relacionamento com Usuário (1:N)
        public int UsuarioId { get; set; }
        [Required]
        public Usuario Usuario { get; set; } = default!;

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
    
    public enum Bancos
    {
        // Bancos Tradicionais
        [Display(Name = "Banco do Brasil")]
        BancoDoBrasil = 1,
        
        [Display(Name = "Itaú")]
        Itau = 2,
        
        [Display(Name = "Bradesco")]
        Bradesco = 3,
        
        [Display(Name = "Caixa Econômica Federal")]
        CaixaEconomica = 4,
        
        [Display(Name = "Santander")]
        Santander = 5,
        
        [Display(Name = "Banco Safra")]
        Safra = 6,

        // Bancos Digitais / Fintechs
        [Display(Name = "Nubank")]
        Nubank = 10,
        
        [Display(Name = "Inter")]
        Inter = 11,
        
        [Display(Name = "C6 Bank")]
        C6Bank = 12,
        
        [Display(Name = "BTG Pactual")]
        BTG = 13,
        
        [Display(Name = "XP Investimentos")]
        XP = 14,
        
        [Display(Name = "Banco Original")]
        Original = 15,

        // Carteiras Digitais e Outros
        [Display(Name = "PicPay")]
        PicPay = 20,
        
        [Display(Name = "PayPal")]
        PayPal = 21,
        
        [Display(Name = "Mercado Pago")]
        MercadoPago = 22,
        
        [Display(Name = "Carteira Física")]
        CarteiraFisica = 30,

        [Display(Name = "Conta Padrão")]
        ContaPadrao = 98,

        [Display(Name = "Outro")]
        Outro = 99
    }
}