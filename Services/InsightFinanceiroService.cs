using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;

namespace OrusFinancas.Services
{
    public class InsightFinanceiroService
    {
        private readonly Contexto _contexto;

        public InsightFinanceiroService(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<string> GerarInsightDiarioAsync(int usuarioId)
        {
            var insights = new List<string>();

            // Análise de gastos do mês
            var mesAtual = DateTime.Now.Month;
            var anoAtual = DateTime.Now.Year;

            var despesasDoMes = await _contexto.Despesas
                .Where(d => d.Conta.Usuario.Id == usuarioId && 
                           d.DataTransacao.Month == mesAtual && 
                           d.DataTransacao.Year == anoAtual)
                .ToListAsync();

            var receitasDoMes = await _contexto.Receitas
                .Where(r => r.Conta.Usuario.Id == usuarioId && 
                           r.DataTransacao.Month == mesAtual && 
                           r.DataTransacao.Year == anoAtual)
                .ToListAsync();

            var totalDespesas = despesasDoMes.Sum(d => d.Valor);
            var totalReceitas = receitasDoMes.Sum(r => r.Valor);
            var saldo = totalReceitas - totalDespesas;

            // Insight baseado no saldo
            if (saldo > 0)
            {
                insights.Add($"Parabéns! Você está com saldo positivo de {saldo:C} neste mês.");
            }
            else if (saldo < 0)
            {
                insights.Add($"Atenção! Você está com saldo negativo de {Math.Abs(saldo):C} neste mês. Considere revisar seus gastos.");
            }

            // Análise de categoria com mais gastos
            if (despesasDoMes.Any())
            {
                var categorias = await _contexto.Categorias
                    .Where(c => c.UsuarioId == usuarioId)
                    .ToListAsync();

                var gastosPorCategoria = despesasDoMes
                    .GroupBy(d => d.CategoriaId)
                    .Select(g => new { 
                        CategoriaId = g.Key, 
                        Total = g.Sum(d => d.Valor) 
                    })
                    .OrderByDescending(x => x.Total)
                    .FirstOrDefault();

                if (gastosPorCategoria != null)
                {
                    var categoria = categorias.FirstOrDefault(c => c.Id == gastosPorCategoria.CategoriaId);
                    if (categoria != null)
                    {
                        insights.Add($"Sua maior despesa está na categoria '{categoria.Nome}' com {gastosPorCategoria.Total:C} gastos neste mês.");
                    }
                }
            }

            // Análise de assinaturas
            var assinaturas = await _contexto.Assinaturas
                .Where(a => a.UsuarioId == usuarioId && a.Ativa)
                .ToListAsync();

            if (assinaturas.Any())
            {
                var totalAssinaturas = assinaturas.Sum(a => a.ValorMensal);
                insights.Add($"Você gasta {totalAssinaturas:C} mensalmente com {assinaturas.Count} assinaturas ativas.");
                
                if (totalAssinaturas > totalReceitas * 0.3m)
                {
                    insights.Add("Suas assinaturas representam mais de 30% da sua renda. Considere cancelar as que não usa.");
                }
            }

            // Dica de economia genérica se não houver dados suficientes
            if (!insights.Any())
            {
                var dicasGenericas = new[]
                {
                    "Registre todas suas despesas para ter maior controle financeiro.",
                    "Defina orçamentos mensais para cada categoria de gasto.",
                    "Reserve pelo menos 10% da sua renda para emergências.",
                    "Revise suas assinaturas mensalmente e cancele as que não usa.",
                    "Compare preços antes de fazer compras importantes."
                };

                var random = new Random();
                insights.Add(dicasGenericas[random.Next(dicasGenericas.Length)]);
            }

            return string.Join(" ", insights);
        }

        public async Task SalvarInsightAsync(int usuarioId, string insight)
        {
            // Verifica se já existe insight para hoje
            var insightExistente = await _contexto.Insights
                .FirstOrDefaultAsync(i => i.UsuarioId == usuarioId && i.DataGeracao.Date == DateTime.Today);

            if (insightExistente == null)
            {
                var novoInsight = new InsightFinanceiro
                {
                    UsuarioId = usuarioId,
                    Titulo = "Insight Diário",
                    Detalhe = insight,
                    DataGeracao = DateTime.Now
                };

                _contexto.Insights.Add(novoInsight);
                await _contexto.SaveChangesAsync();
            }
        }
    }
}