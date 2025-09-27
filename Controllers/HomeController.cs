using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.ViewModels; // Necessário para o ViewModel

namespace OrusFinancas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DashboardService _dashboardService; // Serviço injetado

        public HomeController(ILogger<HomeController> logger, DashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        // Alteração para async Task<IActionResult>
        public async Task<IActionResult> Index()
        {
            // --- ATENÇÃO: Substitua pelo ID real do usuário logado ---
            var usuarioId = 1; 
            // -------------------------------------------------------------

            // Delega a responsabilidade total ao serviço
            HomeDashboardViewModel viewModel = await _dashboardService.GetDashboardDataAsync(usuarioId);
            
            // Retorna o ViewModel fortemente tipado (Sem ViewBags)
            return View(viewModel);
        }
        
        // ... Outros métodos ...
    }
}