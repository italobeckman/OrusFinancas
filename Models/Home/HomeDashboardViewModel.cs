using System.Collections.Generic;
using OrusFinancas.Models; // Para usar Assinatura

namespace OrusFinancas.ViewModels
{
    public class HomeDashboardViewModel
    {
        // === Resumo Financeiro (Balanço) ===
        public decimal ReceitasTotal { get; set; }
        public decimal DespesasTotal { get; set; }
        public decimal BalancoTotal { get; set; }

        // === Maior Despesa ===
        public string MaiorDespesaDescricao { get; set; } = "N/A";
        public decimal MaiorDespesaValor { get; set; }

        // === Insights (Requisito) ===
        // Garante que a string padrão nunca seja nula
        public string InsightDiario { get; set; } = "Nenhum insight inspirador disponível hoje.";

        // === Recorrências (Requisito) ===
        // Lista de objetos Assinatura para exibir na View
        public List<Assinatura> ProximasAssinaturas { get; set; } = new List<Assinatura>();

        // === Orçamentos (Requisito) ===
        // Usa um sub-ViewModel para exibir o status do orçamento
        public List<OrcamentoDashboardItemViewModel> Orcamentos { get; set; } = new List<OrcamentoDashboardItemViewModel>();
    }
    
    // ViewModel Auxiliar para a seção de Orçamentos
    public class OrcamentoDashboardItemViewModel
    {
        public string CategoriaNome { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public decimal Gasto { get; set; }
    }
}