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
                    _logger.LogError(ex, "Erro ao executar tarefas autom�ticas");
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

            _logger.LogInformation("Iniciando processamento de tarefas di�rias �s {DateTime}", DateTime.Now);

            // 1. Processar assinaturas vencidas
            await assinaturaService.GerarTransacoesAssinaturasAsync();
            _logger.LogInformation("Transa��es de assinaturas processadas");

            // 2. Gerar insights di�rios para todos os usu�rios ativos
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
                    _logger.LogError(ex, "Erro ao gerar insight para usu�rio {UsuarioId}", usuarioId);
                }
            }

            _logger.LogInformation("Insights di�rios gerados para {UsuarioCount} usu�rios", usuarios.Count);

            _logger.LogInformation("Tarefas di�rias conclu�das �s {DateTime}", DateTime.Now);
        }
    }
}