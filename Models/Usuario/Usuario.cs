using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace OrusFinancas.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Column("Id")]
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Column("Nome")]
        [Display(Name = "Nome Completo")]
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Column("Email")]
        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(150)]
        [ConcurrencyCheck]
        public string Email { get; set; } = string.Empty;

        [Column("SenhaHash")]
        [Display(Name = "Senha")]
        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        [Column("DataCadastro")]
        [Display(Name = "Data de Cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}