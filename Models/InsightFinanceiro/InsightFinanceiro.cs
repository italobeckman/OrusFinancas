using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("InsightFinanceiro")]
    public class InsightFinanceiro
    {
        public int Id { get; set; }
        
        [Required]
        public string Titulo { get; set; } = string.Empty;
        
        public string Detalhe { get; set; } = string.Empty;
        
        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;

        // Insight pertence a um Usuário (1:N)
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}