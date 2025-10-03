namespace OrusFinancas.Models
{
    // Despesa herda da classe base Transacao
    public class Despesa : Transacao
    {
        // Uma Despesa pode estar associada a uma Assinatura (campo AssinaturaId já existe na classe base)
        // Não precisa de ICollection<Assinatura> aqui - o relacionamento correto é o inverso
    }
}