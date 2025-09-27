using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("PerfilFinanceiro")]
    public class PerfilFinanceiro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal RendaMensal { get; set; }
        
        public string? Metas { get; set; } // Opcional

        // Chave Estrangeira (ForeignKey) e Propriedades de Navegação
        
        // Relacionamento 1:1 com Usuário (Id do Perfil é a Chave Estrangeira)
        // O EF Core usará esta propriedade como FK se o nome não for UserID.
        public int UsuarioId { get; set; } 
        public Usuario? Usuario { get; set; }
    }
}