using System.ComponentModel.DataAnnotations;

namespace OrusFinancas.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string SenhaPura { get; set; } = string.Empty;

        [Display(Name = "Lembrar-me")]
        public bool LembrarMe { get; set; }
    }
}