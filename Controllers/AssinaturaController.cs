using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Services;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class AssinaturaController : Controller
    {
        private readonly Contexto _contexto;
        private readonly AssinaturaService _assinaturaService;

        public AssinaturaController(Contexto contexto, AssinaturaService assinaturaService)
        {
            _contexto = contexto;
            _assinaturaService = assinaturaService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: Assinatura
        public async Task<IActionResult> Index()
        {
            var usuarioId = GetCurrentUserId();
            var assinaturas = await _contexto.Assinaturas
                .Include(a => a.Conta)
                .Where(a => a.UsuarioId == usuarioId)
                .OrderBy(a => a.Servico)
                .ToListAsync();
                
            // Calcular total mensal
            var totalMensal = await _assinaturaService.GetTotalMensalAssinaturasAsync(usuarioId);
            ViewBag.TotalMensal = totalMensal;
            
            return View(assinaturas);
        }

        // GET: Assinatura/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var assinatura = await _contexto.Assinaturas
                .Include(a => a.Conta)
                .Include(a => a.TransacoesGeradas)
                .Where(a => a.Id == id && a.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (assinatura == null) return NotFound();
            return View(assinatura);
        }

        // GET: Assinatura/Create
        public async Task<IActionResult> Create()
        {
            var usuarioId = GetCurrentUserId();
            var contas = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
                
            ViewBag.Contas = contas;
            return View();
        }

        // POST: Assinatura/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assinatura assinatura)
        {
            var usuarioId = GetCurrentUserId();
            assinatura.UsuarioId = usuarioId;

            // Remove campos que não vêm do formulário
            ModelState.Remove("Usuario");
            ModelState.Remove("Conta");
            ModelState.Remove("TransacoesGeradas");
            
            if (ModelState.IsValid)
            {
                _contexto.Add(assinatura);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Assinatura criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            
            // Recarregar contas em caso de erro
            var contas = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
            ViewBag.Contas = contas;
            
            return View(assinatura);
        }

        // GET: Assinatura/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var assinatura = await _contexto.Assinaturas
                .Where(a => a.Id == id && a.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (assinatura == null) return NotFound();
            
            var contas = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
            ViewBag.Contas = contas;
            
            return View(assinatura);
        }

        // POST: Assinatura/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Assinatura assinatura)
        {
            if (id != assinatura.Id) return NotFound();

            var usuarioId = GetCurrentUserId();
            assinatura.UsuarioId = usuarioId;

            // Remove campos que não vêm do formulário
            ModelState.Remove("Usuario");
            ModelState.Remove("Conta");
            ModelState.Remove("TransacoesGeradas");
            
            if (ModelState.IsValid)
            {
                try
                {
                    _contexto.Update(assinatura);
                    await _contexto.SaveChangesAsync();
                    TempData["Success"] = "Assinatura atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Assinaturas.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            
            // Recarregar contas em caso de erro
            var contas = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
            ViewBag.Contas = contas;
            
            return View(assinatura);
        }

        // GET: Assinatura/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var assinatura = await _contexto.Assinaturas
                .Include(a => a.Conta)
                .Where(a => a.Id == id && a.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (assinatura == null) return NotFound();
            return View(assinatura);
        }

        // POST: Assinatura/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = GetCurrentUserId();
            var assinatura = await _contexto.Assinaturas
                .Where(a => a.Id == id && a.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (assinatura != null)
            {
                _contexto.Assinaturas.Remove(assinatura);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Assinatura excluída com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: Assinatura/ToggleAtiva/5
        [HttpPost]
        public async Task<IActionResult> ToggleAtiva(int id)
        {
            var usuarioId = GetCurrentUserId();
            var assinatura = await _contexto.Assinaturas
                .Where(a => a.Id == id && a.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (assinatura != null)
            {
                assinatura.Ativa = !assinatura.Ativa;
                await _contexto.SaveChangesAsync();
                
                var status = assinatura.Ativa ? "ativada" : "desativada";
                TempData["Success"] = $"Assinatura {status} com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
        
        // POST: Gerar despesa manual para assinatura
        [HttpPost]
        public async Task<IActionResult> GerarDespesa(int id)
        {
            var usuarioId = GetCurrentUserId();
            var assinatura = await _contexto.Assinaturas
                .Where(a => a.Id == id && a.UsuarioId == usuarioId && a.Ativa)
                .FirstOrDefaultAsync();

            if (assinatura == null)
            {
                TempData["Error"] = "Assinatura não encontrada ou inativa.";
                return RedirectToAction(nameof(Index));
            }

            var sucesso = await _assinaturaService.GerarDespesaAssinatura(id);
            
            if (sucesso)
            {
                TempData["Success"] = $"Despesa gerada com sucesso para {assinatura.Servico}!";
            }
            else
            {
                TempData["Error"] = "Não foi possível gerar a despesa. Pode já existir uma transação para hoje.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}