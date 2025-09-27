using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.ViewModels;
using System.Linq;
using System.Threading.Tasks;

public class DashboardService
{
    private readonly Contexto _contexto;

    public DashboardService(Contexto contexto)
    {
        _contexto = contexto;
    }

    public async Task<HomeDashboardViewModel> GetDashboardDataAsync(int userId)
    {
        var mesAtual = DateTime.Today.Month;
        var anoAtual = DateTime.Today.Year;
        
        var model = new HomeDashboardViewModel();

        // 1. CÁLCULO DE BALANÇO (Sua Lógica Adaptada)
        var receitas = await _contexto.Receitas
            .Where(r => r.Conta.UsuarioId == userId && r.DataTransacao.Month == mesAtual && r.DataTransacao.Year == anoAtual)
            .SumAsync(r => (decimal?)r.Valor) ?? 0m; 
        
        var despesas = await _contexto.Despesas
            .Where(d => d.Conta.UsuarioId == userId && d.DataTransacao.Month == mesAtual && d.DataTransacao.Year == anoAtual)
            .SumAsync(d => (decimal?)d.Valor) ?? 0m;
        
        var maiorDespesa = await _contexto.Despesas
            .Where(d => d.Conta.UsuarioId == userId && d.DataTransacao.Month == mesAtual && d.DataTransacao.Year == anoAtual)
            .OrderByDescending(d => d.Valor)
            .FirstOrDefaultAsync();

        model.ReceitasTotal = receitas;
        model.DespesasTotal = despesas;
        model.BalancoTotal = receitas - despesas;
        model.MaiorDespesaDescricao = maiorDespesa?.Descricao ?? "N/A";
        model.MaiorDespesaValor = maiorDespesa?.Valor ?? 0m;


        // 2. RECORRÊNCIAS
        // Em DashboardService.cs, no método GetDashboardDataAsync

        model.ProximasAssinaturas = await _contexto.Assinaturas
            .Where(a => a.UsuarioId == userId && a.Ativa)
            // Se a View precisa de Conta ou Categoria, adicione estes includes:
            .OrderBy(a => a.DataAssinatura)
            .Take(5)
            .ToListAsync();


        // 3. INSIGHTS
        var insight = await _contexto.Insights
            .Where(i => i.UsuarioId == userId && i.DataGeracao == DateTime.Today)
            .OrderByDescending(i => i.DataGeracao)
            .FirstOrDefaultAsync();

        model.InsightDiario = insight?.Detalhe ?? "Nenhum insight inspirador disponível hoje.";


        // 4. ORÇAMENTOS (Dados de exemplo)
        // Substitua pelo seu cálculo real de Orçamentos vs. Gastos.
        model.Orcamentos = new List<OrcamentoDashboardItemViewModel>(); 
        
        return model;
    }
}