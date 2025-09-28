using System.ComponentModel.DataAnnotations;

namespace OrusFinancas.Models.ViewModels
{
    public class GastosPorCategoriaViewModel
    {
        [Required]
        public string Categoria { get; set; } = string.Empty;
        
        [Required]
        public decimal Total { get; set; }
    }
}