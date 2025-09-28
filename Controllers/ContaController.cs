using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize] // Adicione isso para garantir que apenas usuários logados acessem
    public class ContaController : Controller
    {
        private readonly Contexto _contexto;

        public ContaController(Contexto contexto)
        {
            _contexto = contexto;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // Ação para exibir a lista de contas (READ - Listagem)
        public async Task<IActionResult> Index()
        {
            var usuarioId = GetCurrentUserId();
            if (usuarioId == 0)
            {
                ModelState.AddModelError(string.Empty, "Erro de autenticação. Por favor, faça o login novamente.");
                return View(new List<Conta>());
            }

            // 1. Busca as contas do usuário
            var contas = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();
                
            // 2. Cálculo do Saldo Atual (Regra de Negócio)
            foreach(var conta in contas)
            {
                var receitas = await _contexto.Receitas
                    .Where(r => r.ContaId == conta.Id).SumAsync(r => (decimal?)r.Valor) ?? 0m;
                var despesas = await _contexto.Despesas
                    .Where(d => d.ContaId == conta.Id).SumAsync(d => (decimal?)d.Valor) ?? 0m;

                conta.SaldoAtual = conta.SaldoInicial + receitas - despesas;
            }

            return View(contas);
        }

        // Ação para exibir o formulário de criação (CREATE - GET)
        public IActionResult Create()
        {
            return View(new Conta());
        }

        // Ação para processar a criação (CREATE - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conta conta)
        {
            var usuarioId = GetCurrentUserId();
            if (usuarioId == 0)
            {
                ModelState.AddModelError(string.Empty, "Erro de autenticação.");
                return View(conta);
            }

            // Definir o usuário para a conta
            conta.UsuarioId = usuarioId;

            // Remover validações de navegação
            ModelState.Remove("Usuario");
            
            if (ModelState.IsValid)
            {
                _contexto.Add(conta);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Conta criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            
            return View(conta);
        }

        // GET: Conta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var conta = await _contexto.Contas
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (conta == null) return NotFound();

            return View(conta);
        }

        // POST: Conta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Conta conta)
        {
            if (id != conta.Id) return NotFound();

            var usuarioId = GetCurrentUserId();
            conta.UsuarioId = usuarioId;

            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                try
                {
                    _contexto.Update(conta);
                    await _contexto.SaveChangesAsync();
                    TempData["Success"] = "Conta atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Contas.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(conta);
        }

        // GET: Conta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var conta = await _contexto.Contas
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (conta == null) return NotFound();

            return View(conta);
        }

        // POST: Conta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = GetCurrentUserId();
            var conta = await _contexto.Contas
                .Where(c => c.Id == id && c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();

            if (conta != null)
            {
                _contexto.Contas.Remove(conta);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Conta excluída com sucesso!";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}