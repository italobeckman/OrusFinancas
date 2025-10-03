using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly Contexto _contexto;

        public CategoriaController(Contexto contexto)
        {
            _contexto = contexto;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: Categoria (Lista todas as categorias do usuário)
        public async Task<IActionResult> Index()
        {
            var usuarioId = GetCurrentUserId();
            
            if (usuarioId == 0)
            {
                return Unauthorized();
            }

            var categorias = await _contexto.Categorias
                .Where(c => c.UsuarioId == usuarioId)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            // Calcular gastos por categoria no mês atual
            var mesAtual = DateTime.Today.Month;
            var anoAtual = DateTime.Today.Year;
            
            var gastosPorCategoria = await _contexto.Despesas
                .Include(d => d.Conta)
                .Where(d => d.Conta.UsuarioId == usuarioId &&
                           d.DataTransacao.Month == mesAtual &&
                           d.DataTransacao.Year == anoAtual &&
                           d.CategoriaId.HasValue)
                .GroupBy(d => d.CategoriaId)
                .Select(g => new 
                {
                    CategoriaId = g.Key.Value,
                    Total = g.Sum(d => d.Valor)
                })
                .ToDictionaryAsync(x => x.CategoriaId, x => x.Total);
            
            var totalGeralGastos = gastosPorCategoria.Values.Sum();
            
            ViewBag.GastosPorCategoria = gastosPorCategoria;
            ViewBag.TotalGeralGastos = totalGeralGastos;

            return View(categorias);
        }

        // GET: Categoria/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var categoria = await _contexto.Categorias
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (categoria == null) return NotFound();
            return View(categoria);
        }

        // GET: Categoria/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            var usuarioId = GetCurrentUserId();
            categoria.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");
            
            if (ModelState.IsValid)
            {
                _contexto.Add(categoria);
                await _contexto.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categoria/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var categoria = await _contexto.Categorias
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (categoria == null) return NotFound();
            return View(categoria);
        }

        // POST: Categoria/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            var usuarioId = GetCurrentUserId();
            categoria.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                try
                {
                    _contexto.Update(categoria);
                    await _contexto.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Categorias.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categoria/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var categoria = await _contexto.Categorias
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (categoria == null) return NotFound();
            return View(categoria);
        }

        // POST: Categoria/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = GetCurrentUserId();
            var categoria = await _contexto.Categorias
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (categoria != null)
            {
                _contexto.Categorias.Remove(categoria);
                await _contexto.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}