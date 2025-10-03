using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;

namespace OrusFinancas.Services
{
    public class OrcamentoService
    {
        private readonly Contexto _contexto;

        public OrcamentoService(Contexto contexto)
        {
            _contexto = contexto;
        }

        /// <summary>
        /// Verifica se algum orçamento atingiu 80% ou 100% do limite
        /// </summary>
        public async Task<List<AlertaOrcamento>> VerificarAlertasOrcamentoAsync(int usuarioId)
        {
            var mesAtual = DateTime.Today.Month;
            var anoAtual = DateTime.Today.Year;
            var alertas = new List<AlertaOrcamento>();

            var orcamentos = await _contexto.Orcamentos
                .Where(o => o.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var orcamento in orcamentos)
            {
                // Calcular gasto atual na categoria do orçamento
                var gastoAtual = await _contexto.Despesas
                    .Include(d => d.Conta)
                    .ThenInclude(c => c.Usuario)
                    .Include(d => d.Categoria)
                    .Where(d => d.Conta.Usuario.Id == usuarioId &&
                               d.DataTransacao.Month == mesAtual &&
                               d.DataTransacao.Year == anoAtual &&
                               d.Categoria != null &&
                               d.Categoria.Nome == orcamento.Nome)
                    .SumAsync(d => (decimal?)d.Valor) ?? 0m;

                var percentualUtilizado = orcamento.ValorLimite > 0 ? (gastoAtual / orcamento.ValorLimite) * 100 : 0;

                if (percentualUtilizado >= 100)
                {
                    alertas.Add(new AlertaOrcamento
                    {
                        OrcamentoNome = orcamento.Nome,
                        Limite = orcamento.ValorLimite,
                        GastoAtual = gastoAtual,
                        PercentualUtilizado = percentualUtilizado,
                        TipoAlerta = TipoAlertaOrcamento.LimiteExcedido,
                        Mensagem = $"?? LIMITE EXCEDIDO! O orçamento '{orcamento.Nome}' ultrapassou 100% do limite ({gastoAtual:C} de {orcamento.ValorLimite:C})"
                    });
                }
                else if (percentualUtilizado >= 80)
                {
                    alertas.Add(new AlertaOrcamento
                    {
                        OrcamentoNome = orcamento.Nome,
                        Limite = orcamento.ValorLimite,
                        GastoAtual = gastoAtual,
                        PercentualUtilizado = percentualUtilizado,
                        TipoAlerta = TipoAlertaOrcamento.ProximoDoLimite,
                        Mensagem = $"? ATENÇÃO! O orçamento '{orcamento.Nome}' atingiu {percentualUtilizado:F0}% do limite ({gastoAtual:C} de {orcamento.ValorLimite:C})"
                    });
                }
            }

            return alertas;
        }

        /// <summary>
        /// Obtém o status detalhado de todos os orçamentos do usuário
        /// </summary>
        public async Task<List<StatusOrcamento>> GetStatusOrcamentosAsync(int usuarioId)
        {
            var mesAtual = DateTime.Today.Month;
            var anoAtual = DateTime.Today.Year;
            var statusList = new List<StatusOrcamento>();

            var orcamentos = await _contexto.Orcamentos
                .Where(o => o.UsuarioId == usuarioId)
                .ToListAsync();

            foreach (var orcamento in orcamentos)
            {
                var gastoAtual = await _contexto.Despesas
                    .Include(d => d.Conta)
                    .ThenInclude(c => c.Usuario)
                    .Include(d => d.Categoria)
                    .Where(d => d.Conta.Usuario.Id == usuarioId &&
                               d.DataTransacao.Month == mesAtual &&
                               d.DataTransacao.Year == anoAtual &&
                               d.Categoria != null &&
                               d.Categoria.Nome == orcamento.Nome)
                    .SumAsync(d => (decimal?)d.Valor) ?? 0m;

                statusList.Add(new StatusOrcamento
                {
                    Orcamento = orcamento,
                    GastoAtual = gastoAtual,
                    ValorRestante = orcamento.ValorLimite - gastoAtual,
                    PercentualUtilizado = orcamento.ValorLimite > 0 ? (gastoAtual / orcamento.ValorLimite) * 100 : 0
                });
            }

            return statusList.OrderByDescending(s => s.PercentualUtilizado).ToList();
        }

        /// <summary>
        /// Cria orçamentos padrão para um novo usuário
        /// </summary>
        public async Task CriarOrcamentosPadraoAsync(int usuarioId)
        {
            var orcamentosPadrao = new[]
            {
                new { Nome = "Alimentação", Valor = 800m },
                new { Nome = "Transporte", Valor = 300m },
                new { Nome = "Lazer", Valor = 200m },
                new { Nome = "Assinaturas", Valor = 100m },
                new { Nome = "Saúde", Valor = 150m }
            };

            foreach (var item in orcamentosPadrao)
            {
                var orcamentoExistente = await _contexto.Orcamentos
                    .AnyAsync(o => o.UsuarioId == usuarioId && o.Nome == item.Nome);

                if (!orcamentoExistente)
                {
                    var orcamento = new Orcamento
                    {
                        Nome = item.Nome,
                        ValorLimite = item.Valor,
                        UsuarioId = usuarioId
                    };

                    _contexto.Orcamentos.Add(orcamento);
                }
            }

            await _contexto.SaveChangesAsync();
        }
    }

    public class AlertaOrcamento
    {
        public string OrcamentoNome { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public decimal GastoAtual { get; set; }
        public decimal PercentualUtilizado { get; set; }
        public TipoAlertaOrcamento TipoAlerta { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class StatusOrcamento
    {
        public Orcamento Orcamento { get; set; } = new();
        public decimal GastoAtual { get; set; }
        public decimal ValorRestante { get; set; }
        public decimal PercentualUtilizado { get; set; }
    }

    public enum TipoAlertaOrcamento
    {
        ProximoDoLimite = 1,
        LimiteExcedido = 2
    }
}