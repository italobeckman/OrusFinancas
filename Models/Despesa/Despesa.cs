namespace OrusFinancas.Models
{
    // Despesa herda da classe base Transacao
    public class Despesa : Transacao
    {
        // Relacionamento 1:N com Assinatura (para Despesa)
        public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
    }
}