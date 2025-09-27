using System.ComponentModel.DataAnnotations;

namespace OrusFinancas.Models.ViewModels // Importante: Crie a pasta ViewModels
{
    public class UsuarioCadastroViewModel
    {
        // Campos do Formulário que serão usados para mapeamento
        
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Senha Pura
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string SenhaPura { get; set; } = string.Empty;

        // Confirmação de Senha (regra de igualdade)
        [DataType(DataType.Password)]
        [Display(Name = "Confirmação de Senha")]
        [Compare("SenhaPura", ErrorMessage = "As senhas não são iguais.")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}