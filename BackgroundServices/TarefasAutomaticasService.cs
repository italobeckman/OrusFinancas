using Microsoft.EntityFrameworkCore;
using OrusFinancas.Services;

namespace OrusFinancas.BackgroundServices
{
    public class TarefasAutomaticasService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TarefasAutomaticasService> _logger;

        public TarefasAutomaticasService(
            IServiceProvider serviceProvider,
            ILogger<TarefasAutomaticasService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessarTarefasDiarias();
                    
                    // Executar a cada 24 horas
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao executar tarefas automáticas");
                    // Em caso de erro, espera 1 hora antes de tentar novamente
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        private async Task ProcessarTarefasDiarias()
        {
            using var scope = _serviceProvider.CreateScope();
            
            var assinaturaService = scope.ServiceProvider.GetRequiredService<AssinaturaService>();
            var insightService = scope.ServiceProvider.GetRequiredService<InsightFinanceiroService>();
            var contexto = scope.ServiceProvider.GetRequiredService<OrusFinancas.Models.Contexto>();

            _logger.LogInformation("Iniciando processamento de tarefas diárias às {DateTime}", DateTime.Now);

            // 1. Processar assinaturas vencidas
            await assinaturaService.GerarTransacoesAssinaturasAsync();
            _logger.LogInformation("Transações de assinaturas processadas");

            // 2. Gerar insights diários para todos os usuários ativos
            var usuarios = await contexto.Usuarios
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var usuarioId in usuarios)
            {
                try
                {
                    var insight = await insightService.GerarInsightDiarioAsync(usuarioId);
                    await insightService.SalvarInsightAsync(usuarioId, insight);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao gerar insight para usuário {UsuarioId}", usuarioId);
                }
            }

            _logger.LogInformation("Insights diários gerados para {UsuarioCount} usuários", usuarios.Count);

            _logger.LogInformation("Tarefas diárias concluídas às {DateTime}", DateTime.Now);
        }
    }
}