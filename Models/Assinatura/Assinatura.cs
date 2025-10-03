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
        
        // Conta onde será debitada a assinatura (opcional)
        public int? ContaId { get; set; }
        public Conta? Conta { get; set; }

        public bool Ativa { get; set; } = true;

        public DateTime DataAssinatura { get; set; } = DateTime.Now;
        
        // Propriedade calculada para o próximo vencimento
        [NotMapped]
        public DateTime ProximoVencimento 
        { 
            get 
            {
                var proximoVencimento = DataAssinatura.AddMonths(1);
                while (proximoVencimento < DateTime.Today)
                {
                    proximoVencimento = proximoVencimento.AddMonths(1);
                }
                return proximoVencimento;
            } 
        }
        
        // Relacionamento com transações (despesas geradas por esta assinatura)
        public ICollection<Transacao> TransacoesGeradas { get; set; } = new List<Transacao>();
    }
}