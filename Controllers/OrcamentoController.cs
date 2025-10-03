using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Services;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class OrcamentoController : Controller
    {
        private readonly Contexto _contexto;
        private readonly OrcamentoService _orcamentoService;

        public OrcamentoController(Contexto contexto, OrcamentoService orcamentoService)
        {
            _contexto = contexto;
            _orcamentoService = orcamentoService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: Orcamento
        public async Task<IActionResult> Index()
        {
            var usuarioId = GetCurrentUserId();
            var statusOrcamentos = await _orcamentoService.GetStatusOrcamentosAsync(usuarioId);
            var alertas = await _orcamentoService.VerificarAlertasOrcamentoAsync(usuarioId);

            ViewBag.Alertas = alertas;
            return View(statusOrcamentos);
        }

        // GET: Orcamento/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var orcamento = await _contexto.Orcamentos
                .Where(o => o.Id == id && o.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (orcamento == null) return NotFound();
            return View(orcamento);
        }

        // GET: Orcamento/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orcamento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Orcamento orcamento)
        {
            var usuarioId = GetCurrentUserId();
            orcamento.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");
            
            if (ModelState.IsValid)
            {
                _contexto.Add(orcamento);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Orçamento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(orcamento);
        }

        // GET: Orcamento/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var orcamento = await _contexto.Orcamentos
                .Where(o => o.Id == id && o.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (orcamento == null) return NotFound();
            return View(orcamento);
        }

        // POST: Orcamento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Orcamento orcamento)
        {
            if (id != orcamento.Id) return NotFound();

            var usuarioId = GetCurrentUserId();
            orcamento.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                try
                {
                    _contexto.Update(orcamento);
                    await _contexto.SaveChangesAsync();
                    TempData["Success"] = "Orçamento atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Orcamentos.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orcamento);
        }

        // GET: Orcamento/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var orcamento = await _contexto.Orcamentos
                .Where(o => o.Id == id && o.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (orcamento == null) return NotFound();
            return View(orcamento);
        }

        // POST: Orcamento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = GetCurrentUserId();
            var orcamento = await _contexto.Orcamentos
                .Where(o => o.Id == id && o.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (orcamento != null)
            {
                _contexto.Orcamentos.Remove(orcamento);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Orçamento excluído com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Criar orçamentos padrão para novos usuários
        [HttpPost]
        public async Task<IActionResult> CriarOrcamentosPadrao()
        {
            var usuarioId = GetCurrentUserId();
            await _orcamentoService.CriarOrcamentosPadraoAsync(usuarioId);
            TempData["Success"] = "Orçamentos padrão criados com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}