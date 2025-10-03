using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrusFinancas.Models
{
    [Table("Tag")]
    public class Tag
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da tag é obrigatório.")]
        [StringLength(30, ErrorMessage = "O nome da tag deve ter no máximo 30 caracteres.")]
        [Display(Name = "Nome da Tag")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(7, ErrorMessage = "A cor deve estar no formato #RRGGBB.")]
        [Display(Name = "Cor (Hex)")]
        public string? Cor { get; set; } // Ex: "#FF5733"

        [StringLength(100)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        // Relacionamento com Usuario (1:N) - Cada tag pertence a um usuário
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Relacionamento Many-to-Many com Transacao
        public ICollection<TransacaoTag> TransacoesTags { get; set; } = new List<TransacaoTag>();
    }
}
