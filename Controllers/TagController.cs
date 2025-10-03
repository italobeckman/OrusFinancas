using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class TagController : Controller
    {
        private readonly Contexto _contexto;

        public TagController(Contexto contexto)
        {
            _contexto = contexto;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: Tag (Lista todas as tags do usuário)
        public async Task<IActionResult> Index()
        {
            var usuarioId = GetCurrentUserId();
            
            if (usuarioId == 0)
            {
                return Unauthorized();
            }

            var tags = await _contexto.Tags
                .Where(t => t.UsuarioId == usuarioId)
                .OrderBy(t => t.Nome)
                .ToListAsync();

            // Calcular quantidade de transações por tag
            var transacoesPorTag = await _contexto.TransacoesTags
                .Include(tt => tt.Transacao)
                    .ThenInclude(t => t.Conta)
                .Where(tt => tt.Transacao.Conta.UsuarioId == usuarioId)
                .GroupBy(tt => tt.TagId)
                .Select(g => new 
                {
                    TagId = g.Key,
                    Quantidade = g.Count()
                })
                .ToDictionaryAsync(x => x.TagId, x => x.Quantidade);
            
            ViewBag.TransacoesPorTag = transacoesPorTag;

            return View(tags);
        }

        // GET: Tag/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var tag = await _contexto.Tags
                .Where(t => t.Id == id && t.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (tag == null) return NotFound();

            // Buscar transações associadas a esta tag
            var transacoes = await _contexto.TransacoesTags
                .Include(tt => tt.Transacao)
                    .ThenInclude(t => t.Conta)
                .Include(tt => tt.Transacao)
                    .ThenInclude(t => t.Categoria)
                .Where(tt => tt.TagId == id && tt.Transacao.Conta.UsuarioId == usuarioId)
                .OrderByDescending(tt => tt.Transacao.DataTransacao)
                .Take(10) // Mostrar últimas 10 transações
                .Select(tt => tt.Transacao)
                .ToListAsync();

            ViewBag.TransacoesRecentes = transacoes;

            return View(tag);
        }

        // GET: Tag/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tag/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tag tag)
        {
            var usuarioId = GetCurrentUserId();
            tag.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");
            
            if (ModelState.IsValid)
            {
                _contexto.Add(tag);
                await _contexto.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tag '{tag.Nome}' criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tag/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var tag = await _contexto.Tags
                .Where(t => t.Id == id && t.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (tag == null) return NotFound();
            return View(tag);
        }

        // POST: Tag/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.Id) return NotFound();

            var usuarioId = GetCurrentUserId();
            tag.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                try
                {
                    _contexto.Update(tag);
                    await _contexto.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Tag '{tag.Nome}' atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Tags.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tag/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var tag = await _contexto.Tags
                .Where(t => t.Id == id && t.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (tag == null) return NotFound();

            // Buscar quantidade de transações usando esta tag
            var quantidadeTransacoes = await _contexto.TransacoesTags
                .CountAsync(tt => tt.TagId == id);

            ViewBag.QuantidadeTransacoes = quantidadeTransacoes;

            return View(tag);
        }

        // POST: Tag/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = GetCurrentUserId();
            var tag = await _contexto.Tags
                .Where(t => t.Id == id && t.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (tag != null)
            {
                // As associações em TransacaoTag serão removidas automaticamente (Cascade Delete)
                _contexto.Tags.Remove(tag);
                await _contexto.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tag '{tag.Nome}' excluída com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}
