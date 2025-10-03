namespace OrusFinancas.Models
{
    // Despesa herda da classe base Transacao
    public class Despesa : Transacao
    {
        // Uma Despesa pode estar associada a uma Assinatura (campo AssinaturaId j� existe na classe base)
        // N�o precisa de ICollection<Assinatura> aqui - o relacionamento correto � o inverso
    }
}