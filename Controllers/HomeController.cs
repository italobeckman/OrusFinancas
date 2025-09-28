using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.ViewModels;
using OrusFinancas.Services;

namespace OrusFinancas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DashboardService _dashboardService;
        private readonly AssinaturaService _assinaturaService;

        public HomeController(ILogger<HomeController> logger, DashboardService dashboardService, AssinaturaService assinaturaService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _assinaturaService = assinaturaService;
        }

        public async Task<IActionResult> Index()
        {
            // Se o usuário não está autenticado, redireciona para a página de boas-vindas
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Welcome", "Auth");
            }

            // Obtém o ID do usuário logado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                _logger.LogWarning("Usuário sem ID válido tentou acessar o dashboard");
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                _logger.LogInformation("Usuário {UserId} ({UserName}) acessando dashboard", usuarioId, User.Identity.Name);
                
                // Gerar despesas automáticas de assinaturas antes de carregar o dashboard
                try
                {
                    await _assinaturaService.GerarTransacoesAssinaturasAsync();
                    _logger.LogInformation("Transações de assinaturas processadas para usuário {UserId}", usuarioId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao processar assinaturas para usuário {UserId}, continuando sem elas", usuarioId);
                    // Continua mesmo com erro nas assinaturas
                }
                
                // Delega a responsabilidade total ao serviço
                HomeDashboardViewModel viewModel = await _dashboardService.GetDashboardDataAsync(usuarioId);
                
                // Adicionar informações do usuário ao ViewBag
                ViewBag.UsuarioNome = User.Identity.Name;
                ViewBag.UsuarioId = usuarioId;
                
                _logger.LogInformation("Dashboard carregado com sucesso para usuário {UserId}", usuarioId);
                
                // Retorna o ViewModel fortemente tipado
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao carregar dashboard para usuário {UserId}: {ErrorMessage}", usuarioId, ex.Message);
                
                // Em caso de erro, retornar um ViewModel básico funcional
                var emptyViewModel = new HomeDashboardViewModel
                {
                    ReceitasTotal = 0,
                    DespesasTotal = 0,
                    BalancoTotal = 0,
                    MaiorDespesaDescricao = "Sistema temporariamente indisponível",
                    MaiorDespesaValor = 0,
                    InsightDiario = "⚠️ Ocorreu um problema ao carregar seus dados. Por favor, tente novamente em alguns minutos.",
                    ProximasAssinaturas = new List<OrusFinancas.Models.Assinatura>(),
                    Orcamentos = new List<OrcamentoDashboardItemViewModel>()
                };
                
                TempData["Error"] = "Ocorreu um erro ao carregar o dashboard. Os dados podem estar temporariamente indisponíveis.";
                
                // Adicionar informações do usuário mesmo em caso de erro
                ViewBag.UsuarioNome = User.Identity.Name ?? "Usuário";
                ViewBag.UsuarioId = usuarioId;
                
                return View(emptyViewModel);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Action para obter estatísticas rápidas via AJAX
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<JsonResult> GetQuickStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                return Json(new { success = false, message = "Usuário não encontrado" });
            }

            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync(usuarioId);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas para usuário {UserId}", usuarioId);
                return Json(new { success = false, message = "Erro interno do servidor" });
            }
        }
        
        /// <summary>
        /// Action para processar assinaturas e atualizar dados
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ProcessarAssinaturas()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                TempData["Error"] = "Erro de autenticação.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _logger.LogInformation("Processamento manual de assinaturas iniciado por usuário {UserId}", usuarioId);
                await _assinaturaService.GerarTransacoesAssinaturasAsync();
                TempData["Success"] = "Assinaturas processadas com sucesso!";
                _logger.LogInformation("Processamento manual de assinaturas concluído para usuário {UserId}", usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar assinaturas manualmente para usuário {UserId}", usuarioId);
                TempData["Error"] = "Erro ao processar assinaturas automáticas. Tente novamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Action para diagnóstico do sistema
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<JsonResult> DiagnosticoSistema()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                return Json(new { success = false, message = "Usuário não encontrado" });
            }

            try
            {
                var diagnostico = new
                {
                    UserId = usuarioId,
                    UserName = User.Identity.Name,
                    NeedsSetup = await _dashboardService.NeedsInitialSetupAsync(usuarioId),
                    Stats = await _dashboardService.GetDashboardStatsAsync(usuarioId),
                    Timestamp = DateTime.Now
                };

                return Json(new { success = true, data = diagnostico });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no diagnóstico do sistema para usuário {UserId}", usuarioId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Action para teste básico do sistema
        /// </summary>
        [Authorize]
        public async Task<IActionResult> TesteConexao()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
            {
                return Json(new { success = false, message = "Usuário não encontrado" });
            }

            try
            {
                // Teste usando o service
                var stats = await _dashboardService.GetDashboardStatsAsync(usuarioId);
                var needsSetup = await _dashboardService.NeedsInitialSetupAsync(usuarioId);

                return Json(new { 
                    success = true, 
                    data = new { 
                        userId = usuarioId,
                        userName = User.Identity?.Name ?? "N/A",
                        stats = stats,
                        needsSetup = needsSetup,
                        timestamp = DateTime.Now
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no teste de conexão para usuário {UserId}", usuarioId);
                return Json(new { 
                    success = false, 
                    message = ex.Message, 
                    innerException = ex.InnerException?.Message,
                    source = ex.Source
                });
            }
        }

        public IActionResult Diagnostico()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            return View();
        }
    }
}