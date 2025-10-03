using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Services;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class InsightController : Controller
    {
        private readonly Contexto _contexto;
        private readonly InsightFinanceiroService _insightService;

        public InsightController(Contexto contexto, InsightFinanceiroService insightService)
        {
            _contexto = contexto;
            _insightService = insightService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: Insight
        public async Task<IActionResult> Index(int pagina = 1)
        {
            var usuarioId = GetCurrentUserId();
            const int itensPorPagina = 10;

            var insights = await _contexto.Insights
                .Where(i => i.UsuarioId == usuarioId)
                .OrderByDescending(i => i.DataGeracao)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            var totalItens = await _contexto.Insights
                .CountAsync(i => i.UsuarioId == usuarioId);

            ViewBag.PaginaAtual = pagina;
            ViewBag.TotalPaginas = Math.Ceiling((double)totalItens / itensPorPagina);

            return View(insights);
        }

        // POST: Gerar insight manual
        [HttpPost]
        public async Task<IActionResult> GerarInsight()
        {
            var usuarioId = GetCurrentUserId();
            
            try
            {
                var insight = await _insightService.GerarInsightDiarioAsync(usuarioId);
                await _insightService.SalvarInsightAsync(usuarioId, insight);
                
                TempData["Success"] = "Insight gerado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao gerar insight: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Insight/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var insight = await _contexto.Insights
                .Where(i => i.Id == id && i.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (insight == null) return NotFound();

            return View(insight);
        }
    }
}