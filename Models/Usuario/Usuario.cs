using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace OrusFinancas.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        // Campos que VÃO para o banco de dados (Entidade)
        
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // ESTE É O CAMPO DE SEGURANÇA. Guarda o hash, não a senha pura.
        [Column("SenhaHash")] 
        public string SenhaHash { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        
        // Coloque aqui todas as suas propriedades de navegação (ICollection<T> e outros)
        // public ICollection<Conta> Contas { get; set; } = new List<Conta>();
    }
}