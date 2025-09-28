using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrusFinancas.Services
{
    public class DashboardService
    {
        private readonly Contexto _contexto;
        private readonly OrcamentoService _orcamentoService;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(Contexto contexto, OrcamentoService orcamentoService, ILogger<DashboardService> logger)
        {
            _contexto = contexto;
            _orcamentoService = orcamentoService;
            _logger = logger;
        }

        public async Task<HomeDashboardViewModel> GetDashboardDataAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Iniciando carregamento de dashboard para usu�rio {UserId}", userId);
                
                var mesAtual = DateTime.Today.Month;
                var anoAtual = DateTime.Today.Year;
                
                var model = new HomeDashboardViewModel();

                // Verificar se � um usu�rio novo
                var isNewUser = await IsNewUserAsync(userId);
                _logger.LogInformation("Usu�rio {UserId} � novo: {IsNew}", userId, isNewUser);

                // 1. C�LCULO DE BALAN�O - Usando consultas mais simples
                await CarregarResumoFinanceiroAsync(model, userId, mesAtual, anoAtual, isNewUser);
                
                // 2. PR�XIMAS ASSINATURAS
                await CarregarProximasAssinaturasAsync(model, userId);

                // 3. INSIGHTS
                await CarregarInsightAsync(model, userId, isNewUser);

                // 4. OR�AMENTOS
                await CarregarOrcamentosAsync(model, userId, mesAtual, anoAtual);
                
                _logger.LogInformation("Dashboard carregado com sucesso para usu�rio {UserId}", userId);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard para usu�rio {UserId}", userId);
                
                // Retornar um modelo b�sico em caso de erro
                return new HomeDashboardViewModel
                {
                    InsightDiario = "Ocorreu um erro ao carregar os dados. Verifique sua conex�o e tente novamente.",
                    ProximasAssinaturas = new List<Assinatura>(),
                    Orcamentos = new List<OrcamentoDashboardItemViewModel>()
                };
            }
        }

        private async Task<bool> IsNewUserAsync(int userId)
        {
            try
            {
                var hasAccounts = await _contexto.Contas.AnyAsync(c => c.UsuarioId == userId);
                var hasTransactions = await _contexto.Transacoes
                    .AnyAsync(t => _contexto.Contas.Any(c => c.Id == t.ContaId && c.UsuarioId == userId));
                    
                return !hasAccounts && !hasTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se usu�rio {UserId} � novo", userId);
                return false;
            }
        }

        private async Task CarregarResumoFinanceiroAsync(HomeDashboardViewModel model, int userId, int mes, int ano, bool isNewUser)
        {
            try
            {
                // Buscar contas do usu�rio primeiro
                var contasIds = await _contexto.Contas
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (!contasIds.Any())
                {
                    model.ReceitasTotal = 0;
                    model.DespesasTotal = 0;
                    model.BalancoTotal = 0;
                    model.MaiorDespesaDescricao = isNewUser ? "Crie sua primeira conta para come�ar!" : "Nenhuma conta cadastrada";
                    model.MaiorDespesaValor = 0;
                    return;
                }

                // Calcular receitas
                var receitas = await _contexto.Receitas
                    .Where(r => contasIds.Contains(r.ContaId) && 
                               r.DataTransacao.Month == mes && 
                               r.DataTransacao.Year == ano)
                    .SumAsync(r => (decimal?)r.Valor) ?? 0m;

                // Calcular despesas
                var despesas = await _contexto.Despesas
                    .Where(d => contasIds.Contains(d.ContaId) && 
                               d.DataTransacao.Month == mes && 
                               d.DataTransacao.Year == ano)
                    .SumAsync(d => (decimal?)d.Valor) ?? 0m;

                // Buscar maior despesa
                var maiorDespesa = await _contexto.Despesas
                    .Where(d => contasIds.Contains(d.ContaId) && 
                               d.DataTransacao.Month == mes && 
                               d.DataTransacao.Year == ano)
                    .OrderByDescending(d => d.Valor)
                    .FirstOrDefaultAsync();

                model.ReceitasTotal = receitas;
                model.DespesasTotal = despesas;
                model.BalancoTotal = receitas - despesas;
                model.MaiorDespesaDescricao = maiorDespesa?.Descricao ?? 
                    (isNewUser ? "Adicione sua primeira transa��o!" : "Nenhuma despesa registrada este m�s");
                model.MaiorDespesaValor = maiorDespesa?.Valor ?? 0m;

                _logger.LogInformation("Resumo financeiro carregado: Receitas={Receitas}, Despesas={Despesas}", receitas, despesas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar resumo financeiro para usu�rio {UserId}", userId);
                model.ReceitasTotal = 0;
                model.DespesasTotal = 0;
                model.BalancoTotal = 0;
                model.MaiorDespesaDescricao = "Erro ao carregar dados financeiros";
                model.MaiorDespesaValor = 0;
            }
        }

        private async Task CarregarProximasAssinaturasAsync(HomeDashboardViewModel model, int userId)
        {
            try
            {
                var proximasAssinaturas = await _contexto.Assinaturas
                    .Where(a => a.UsuarioId == userId && a.Ativa)
                    .OrderBy(a => a.DataAssinatura.Day)
                    .Take(5)
                    .ToListAsync();

                model.ProximasAssinaturas = proximasAssinaturas;
                _logger.LogInformation("Carregadas {Count} assinaturas pr�ximas", proximasAssinaturas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pr�ximas assinaturas para usu�rio {UserId}", userId);
                model.ProximasAssinaturas = new List<Assinatura>();
            }
        }

        private async Task CarregarInsightAsync(HomeDashboardViewModel model, int userId, bool isNewUser)
        {
            try
            {
                if (isNewUser)
                {
                    model.InsightDiario = "?? Bem-vindo ao �rus Finan�as! Comece criando uma conta e registrando suas primeiras transa��es para receber insights personalizados.";
                    return;
                }

                var insightRecente = await _contexto.Insights
                    .Where(i => i.UsuarioId == userId)
                    .OrderByDescending(i => i.DataGeracao)
                    .FirstOrDefaultAsync();

                if (insightRecente != null)
                {
                    if (insightRecente.DataGeracao.Date == DateTime.Today)
                    {
                        model.InsightDiario = $"?? {insightRecente.Detalhe}";
                    }
                    else
                    {
                        model.InsightDiario = $"?? �ltimo insight ({insightRecente.DataGeracao:dd/MM}): {insightRecente.Detalhe}";
                    }
                }
                else
                {
                    // Verificar se h� dados suficientes
                    var contasIds = await _contexto.Contas
                        .Where(c => c.UsuarioId == userId)
                        .Select(c => c.Id)
                        .ToListAsync();

                    var temDados = contasIds.Any() && await _contexto.Transacoes
                        .AnyAsync(t => contasIds.Contains(t.ContaId));

                    if (temDados)
                    {
                        model.InsightDiario = "?? Clique em 'Gerar Novo Insight' para receber dicas personalizadas baseadas em suas transa��es!";
                    }
                    else
                    {
                        model.InsightDiario = "?? Adicione mais transa��es para receber insights inteligentes sobre seus h�bitos financeiros.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar insight para usu�rio {UserId}", userId);
                model.InsightDiario = "?? Sistema de insights temporariamente indispon�vel.";
            }
        }

        private async Task CarregarOrcamentosAsync(HomeDashboardViewModel model, int userId, int mes, int ano)
        {
            try
            {
                var orcamentos = await _contexto.Orcamentos
                    .Where(o => o.UsuarioId == userId)
                    .ToListAsync();

                if (!orcamentos.Any())
                {
                    // Verificar se deve criar or�amentos padr�o
                    var contasIds = await _contexto.Contas
                        .Where(c => c.UsuarioId == userId)
                        .Select(c => c.Id)
                        .ToListAsync();

                    var hasTransactions = contasIds.Any() && await _contexto.Transacoes
                        .AnyAsync(t => contasIds.Contains(t.ContaId));

                    if (hasTransactions)
                    {
                        try
                        {
                            await _orcamentoService.CriarOrcamentosPadraoAsync(userId);
                            orcamentos = await _contexto.Orcamentos
                                .Where(o => o.UsuarioId == userId)
                                .ToListAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao criar or�amentos padr�o para usu�rio {UserId}", userId);
                        }
                    }
                }

                var orcamentosViewModel = new List<OrcamentoDashboardItemViewModel>();

                foreach (var orcamento in orcamentos.Take(5))
                {
                    var gastoNaCategoria = await CalcularGastoOrcamento(userId, orcamento.Nome, mes, ano);

                    orcamentosViewModel.Add(new OrcamentoDashboardItemViewModel
                    {
                        CategoriaNome = orcamento.Nome,
                        Limite = orcamento.ValorLimite,
                        Gasto = gastoNaCategoria
                    });
                }

                model.Orcamentos = orcamentosViewModel.OrderByDescending(o => 
                    o.Limite > 0 ? o.Gasto / o.Limite : 0).ToList();

                _logger.LogInformation("Carregados {Count} or�amentos", orcamentosViewModel.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar or�amentos para usu�rio {UserId}", userId);
                model.Orcamentos = new List<OrcamentoDashboardItemViewModel>();
            }
        }

        private async Task<decimal> CalcularGastoOrcamento(int userId, string nomeOrcamento, int mes, int ano)
        {
            try
            {
                var contasIds = await _contexto.Contas
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                if (!contasIds.Any()) return 0m;

                // Buscar gastos por categoria exata
                var gastoCategoria = await _contexto.Despesas
                    .Where(d => contasIds.Contains(d.ContaId) &&
                               d.DataTransacao.Month == mes &&
                               d.DataTransacao.Year == ano &&
                               d.Categoria != null &&
                               (d.Categoria.Nome.ToLower() == nomeOrcamento.ToLower() ||
                                d.Categoria.Nome.ToLower().Contains(nomeOrcamento.ToLower()) ||
                                nomeOrcamento.ToLower().Contains(d.Categoria.Nome.ToLower())))
                    .SumAsync(d => (decimal?)d.Valor) ?? 0m;

                if (gastoCategoria > 0) return gastoCategoria;

                // Buscar por palavras-chave na descri��o
                return await CalcularGastoPorPalavrasChave(userId, nomeOrcamento, mes, ano, contasIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular gasto do or�amento {Orcamento}", nomeOrcamento);
                return 0m;
            }
        }

        private async Task<decimal> CalcularGastoPorPalavrasChave(int userId, string nomeOrcamento, int mes, int ano, List<int> contasIds)
        {
            var palavrasChave = new Dictionary<string, string[]>
            {
                { "alimenta��o", new[] { "alimenta��o", "comida", "supermercado", "restaurante", "delivery", "ifood" } },
                { "transporte", new[] { "transporte", "combust�vel", "gasolina", "uber", "taxi", "�nibus", "metro" } },
                { "lazer", new[] { "lazer", "entretenimento", "cinema", "divers�o", "streaming", "jogo" } },
                { "assinatura", new[] { "assinatura", "netflix", "spotify", "amazon", "mensalidade" } },
                { "sa�de", new[] { "sa�de", "m�dico", "farm�cia", "hospital", "consulta", "rem�dio", "plano" } }
            };

            var chaves = palavrasChave
                .Where(kv => nomeOrcamento.ToLower().Contains(kv.Key))
                .SelectMany(kv => kv.Value)
                .ToArray();

            if (!chaves.Any())
                chaves = new[] { nomeOrcamento.ToLower() };

            var gasto = 0m;
            
            foreach (var chave in chaves)
            {
                var gastoChave = await _contexto.Despesas
                    .Where(d => contasIds.Contains(d.ContaId) &&
                               d.DataTransacao.Month == mes &&
                               d.DataTransacao.Year == ano &&
                               d.Descricao.ToLower().Contains(chave))
                    .SumAsync(d => (decimal?)d.Valor) ?? 0m;
                    
                if (gastoChave > gasto)
                    gasto = gastoChave;
            }

            return gasto;
        }

        public async Task<DashboardStatsViewModel> GetDashboardStatsAsync(int userId)
        {
            try
            {
                var stats = new DashboardStatsViewModel();
                var dataInicio = DateTime.Today.AddDays(-30);
                
                var contasIds = await _contexto.Contas
                    .Where(c => c.UsuarioId == userId)
                    .Select(c => c.Id)
                    .ToListAsync();

                stats.TotalContas = contasIds.Count;
                
                stats.TotalTransacoes = contasIds.Any() ? await _contexto.Transacoes
                    .CountAsync(t => contasIds.Contains(t.ContaId) && t.DataTransacao >= dataInicio) : 0;
                    
                stats.TotalAssinaturasAtivas = await _contexto.Assinaturas
                    .CountAsync(a => a.UsuarioId == userId && a.Ativa);
                    
                stats.TotalCategorias = await _contexto.Categorias
                    .CountAsync(c => c.UsuarioId == userId);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estat�sticas para usu�rio {UserId}", userId);
                return new DashboardStatsViewModel();
            }
        }

        public async Task<bool> NeedsInitialSetupAsync(int userId)
        {
            try
            {
                var hasAccounts = await _contexto.Contas.AnyAsync(c => c.UsuarioId == userId);
                var hasCategories = await _contexto.Categorias.AnyAsync(c => c.UsuarioId == userId);
                
                return !hasAccounts || !hasCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar setup inicial para usu�rio {UserId}", userId);
                return false;
            }
        }
    }

    public class DashboardStatsViewModel
    {
        public int TotalContas { get; set; }
        public int TotalTransacoes { get; set; }
        public int TotalAssinaturasAtivas { get; set; }
        public int TotalCategorias { get; set; }
    }
}