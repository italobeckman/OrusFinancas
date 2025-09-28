using System.ComponentModel.DataAnnotations;

namespace OrusFinancas.Models
{
    // Receita herda da classe base Transacao
    public class Receita : Transacao
    {
        // Propriedades específicas de Receita
        public TipoReceita TipoReceita { get; set; } = TipoReceita.Outras;
        
        // Override para garantir que receitas não tenham assinatura
        public new int? AssinaturaId 
        { 
            get => null; 
            set { /* Ignorar - receitas não têm assinaturas */ } 
        }
        
        public new Assinatura? Assinatura 
        { 
            get => null; 
            set { /* Ignorar - receitas não têm assinaturas */ } 
        }
    }
    
    public enum TipoReceita
    {
        [Display(Name = "Salário")]
        Salario = 1,
        
        [Display(Name = "Freelance")]
        Freelance = 2,
        
        [Display(Name = "Investimentos")]
        Investimentos = 3,
        
        [Display(Name = "Vendas")]
        Vendas = 4,
        
        [Display(Name = "Bonificação")]
        Bonificacao = 5,
        
        [Display(Name = "13º Salário")]
        DecimoTerceiro = 6,
        
        [Display(Name = "Férias")]
        Ferias = 7,
        
        [Display(Name = "Outras")]
        Outras = 99
    }
}