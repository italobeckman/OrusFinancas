using System.ComponentModel.DataAnnotations;

namespace OrusFinancas.Models.ViewModels
{
    public class TransacaoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        public DateTime DataTransacao { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Selecione uma conta")]
        public int ContaId { get; set; }

        // Categoria agora é opcional para receitas
        public int? CategoriaId { get; set; }

        // Apenas para despesas
        public int? AssinaturaId { get; set; }

        [Required(ErrorMessage = "Selecione o tipo de transação")]
        public TipoTransacao TipoTransacao { get; set; }
        
        // Para receitas - substitui categoria
        public TipoReceita? TipoReceita { get; set; }

        // Propriedades para os dropdowns
        public List<Conta> Contas { get; set; } = new List<Conta>();
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();
        public List<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
        
        // Validação customizada
        public bool IsValid()
        {
            if (TipoTransacao == TipoTransacao.Despesa && !CategoriaId.HasValue)
            {
                return false; // Despesas precisam de categoria
            }
            
            if (TipoTransacao == TipoTransacao.Receita && !TipoReceita.HasValue)
            {
                return false; // Receitas precisam de tipo
            }
            
            return true;
        }
    }

    public enum TipoTransacao
    {
        [Display(Name = "Receita")]
        Receita = 1,
        [Display(Name = "Despesa")]
        Despesa = 2
    }
}