using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.Services;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class TransacaoController : Controller
    {
        private readonly Contexto _contexto;
        private readonly SeedDataService _seedDataService;

        public TransacaoController(Contexto contexto, SeedDataService seedDataService)
        {
            _contexto = contexto;
            _seedDataService = seedDataService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // GET: Transacao
        public async Task<IActionResult> Index()
        {
            var usuarioId = GetCurrentUserId();
            var transacoes = await _contexto.Transacoes
                .Include(t => t.Conta)
                .Include(t => t.Categoria)
                .Include(t => t.Assinatura)
                .Include(t => t.TransacoesTags)
                    .ThenInclude(tt => tt.Tag)
                .Where(t => t.Conta.Usuario.Id == usuarioId)
                .OrderByDescending(t => t.DataTransacao)
                .ToListAsync();
            
            return View(transacoes);
        }

        // GET: Transacao/Create
        public async Task<IActionResult> Create()
        {
            var usuarioId = GetCurrentUserId();
            
            // Criar categorias básicas se o usuário não tiver nenhuma
            await _seedDataService.CriarCategoriasBasicasAsync(usuarioId);
            
            var viewModel = new TransacaoViewModel
            {
                Contas = await _contexto.Contas.Where(c => c.Usuario.Id == usuarioId).ToListAsync(),
                Categorias = await _contexto.Categorias
                    .Where(c => c.UsuarioId == usuarioId)
                    .ToListAsync(), // Garante que só traz categorias do usuário logado
                Assinaturas = await _contexto.Assinaturas.Where(a => a.UsuarioId == usuarioId && a.Ativa).ToListAsync(),
                Tags = await _contexto.Tags.Where(t => t.UsuarioId == usuarioId).OrderBy(t => t.Nome).ToListAsync()
            };
            
            return View(viewModel);
        }

        // POST: Transacao/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransacaoViewModel viewModel)
        {
            var usuarioId = GetCurrentUserId();

            // Validações customizadas (manter o que já existe)
            if (viewModel.TipoTransacao == TipoTransacao.Despesa && !viewModel.CategoriaId.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.CategoriaId), "Categoria é obrigatória para despesas.");
            }

            if (viewModel.TipoTransacao == TipoTransacao.Receita && !viewModel.TipoReceita.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.TipoReceita), "Tipo de receita é obrigatório.");
            }

            // Validação e carregamento da conta
            // CRÍTICO: Carregue a entidade Conta para garantir que ela seja rastreada
            var conta = await _contexto.Contas.Where(c => c.Id == viewModel.ContaId && c.Usuario.Id == usuarioId).FirstOrDefaultAsync();
            if (conta == null)
            {
                ModelState.AddModelError(nameof(viewModel.ContaId), "Conta inválida para este usuário.");
            }

            if (ModelState.IsValid)
            {
                Transacao transacao;


                if (viewModel.TipoTransacao == TipoTransacao.Receita)
                {
                    transacao = new Receita
                    {
                        TipoReceita = viewModel.TipoReceita ?? TipoReceita.Outras,
                        CategoriaId = null, // Receitas não têm categoria
                        AssinaturaId = null // nem assinatura
                    };
                }
                else
                {
                    transacao = new Despesa();
                    transacao.CategoriaId = viewModel.CategoriaId;
                    transacao.AssinaturaId = viewModel.AssinaturaId;
                }

                // Atribuir as propriedades
                transacao.Descricao = viewModel.Descricao;
                transacao.Valor = viewModel.Valor;
                transacao.DataTransacao = viewModel.DataTransacao;

                // A MUDANÇA MAIS IMPORTANTE:
                // Atribua a entidade Conta carregada à propriedade de navegação.
                // Isso instrui o EF Core a usar a Conta existente.
                transacao.Conta = conta;

                _contexto.Add(transacao);
                await _contexto.SaveChangesAsync();

                // Associar tags selecionadas
                if (viewModel.TagsSelecionadas != null && viewModel.TagsSelecionadas.Any())
                {
                    foreach (var tagId in viewModel.TagsSelecionadas)
                    {
                        var transacaoTag = new TransacaoTag
                        {
                            TransacaoId = transacao.Id,
                            TagId = tagId,
                            DataAssociacao = DateTime.Now
                        };
                        _contexto.TransacoesTags.Add(transacaoTag);
                    }
                    await _contexto.SaveChangesAsync();
                }

                TempData["Success"] = $"{viewModel.TipoTransacao} de {viewModel.Valor:C} criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            // Recarregar listas se houver erro
            viewModel.Contas = await _contexto.Contas.Where(c => c.Usuario.Id == usuarioId).ToListAsync();
            // Atenção: Seu código tem duas chamadas `.Where()` para categorias, o que é um erro.
            // Mantenha apenas uma chamada.
            viewModel.Categorias = await _contexto.Categorias
                .Where(c => c.UsuarioId == usuarioId && c.TipoCategoria == TipoCategoria.Despesa)
                .ToListAsync();
            viewModel.Assinaturas = await _contexto.Assinaturas.Where(a => a.UsuarioId == usuarioId && a.Ativa).ToListAsync();
            viewModel.Tags = await _contexto.Tags.Where(t => t.UsuarioId == usuarioId).OrderBy(t => t.Nome).ToListAsync();

            return View(viewModel);
        }

        // GET: Transacao/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var transacao = await _contexto.Transacoes
                .Include(t => t.Conta)
                .Include(t => t.TransacoesTags)
                .Where(t => t.Id == id && t.Conta.Usuario.Id == usuarioId)
                .FirstOrDefaultAsync();

            if (transacao == null) return NotFound();

            var viewModel = new TransacaoViewModel
            {
                Id = transacao.Id,
                Descricao = transacao.Descricao,
                Valor = transacao.Valor,
                DataTransacao = transacao.DataTransacao,
                ContaId = transacao.ContaId,
                CategoriaId = transacao.CategoriaId,
                AssinaturaId = transacao.AssinaturaId,
                TipoTransacao = transacao is Receita ? TipoTransacao.Receita : TipoTransacao.Despesa,
                TagsSelecionadas = transacao.TransacoesTags.Select(tt => tt.TagId).ToList(),
                Contas = await _contexto.Contas.Where(c => c.Usuario.Id == usuarioId).ToListAsync(),
                Categorias = await _contexto.Categorias
                    .Where(c => c.UsuarioId == usuarioId && c.TipoCategoria == TipoCategoria.Despesa)
                    .ToListAsync(),
                Assinaturas = await _contexto.Assinaturas.Where(a => a.UsuarioId == usuarioId && a.Ativa).ToListAsync(),
                Tags = await _contexto.Tags.Where(t => t.UsuarioId == usuarioId).OrderBy(t => t.Nome).ToListAsync()
            };
            
            // Se for receita, adicionar o tipo
            if (transacao is Receita receita)
            {
                viewModel.TipoReceita = receita.TipoReceita;
            }

            return View(viewModel);
        }

        // POST: Transacao/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TransacaoViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            var usuarioId = GetCurrentUserId();
            
            // Validações customizadas
            if (viewModel.TipoTransacao == TipoTransacao.Despesa && !viewModel.CategoriaId.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.CategoriaId), "Categoria é obrigatória para despesas.");
            }
            
            if (viewModel.TipoTransacao == TipoTransacao.Receita && !viewModel.TipoReceita.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.TipoReceita), "Tipo de receita é obrigatório.");
            }

            if (ModelState.IsValid)
            {
                var transacao = await _contexto.Transacoes
                    .Include(t => t.Conta)
                    .Include(t => t.TransacoesTags)
                    .Where(t => t.Id == id && t.Conta.Usuario.Id == usuarioId)
                    .FirstOrDefaultAsync();

                if (transacao == null) return NotFound();

                transacao.Descricao = viewModel.Descricao;
                transacao.Valor = viewModel.Valor;
                transacao.DataTransacao = viewModel.DataTransacao;
                transacao.ContaId = viewModel.ContaId;

                // Atualizar campos específicos
                if (transacao is Receita receita)
                {
                    receita.TipoReceita = viewModel.TipoReceita ?? TipoReceita.Outras;
                    transacao.CategoriaId = null;
                    transacao.AssinaturaId = null;
                }
                else
                {
                    transacao.CategoriaId = viewModel.CategoriaId;
                    transacao.AssinaturaId = viewModel.AssinaturaId;
                }

                // Atualizar tags associadas
                // Remover tags antigas
                var tagsAntigas = transacao.TransacoesTags.ToList();
                _contexto.TransacoesTags.RemoveRange(tagsAntigas);

                // Adicionar novas tags selecionadas
                if (viewModel.TagsSelecionadas != null && viewModel.TagsSelecionadas.Any())
                {
                    foreach (var tagId in viewModel.TagsSelecionadas)
                    {
                        var transacaoTag = new TransacaoTag
                        {
                            TransacaoId = transacao.Id,
                            TagId = tagId,
                            DataAssociacao = DateTime.Now
                        };
                        _contexto.TransacoesTags.Add(transacaoTag);
                    }
                }

                try
                {
                    _contexto.Update(transacao);
                    await _contexto.SaveChangesAsync();
                    TempData["Success"] = "Transação atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Transacoes.Any(e => e.Id == id))
                        return NotFound();
                    throw;
                }
            }

            // Recarregar listas se houver erro
            viewModel.Contas = await _contexto.Contas.Where(c => c.Usuario.Id == usuarioId).ToListAsync();
            viewModel.Categorias = await _contexto.Categorias
                .Where(c => c.UsuarioId == usuarioId && c.TipoCategoria == TipoCategoria.Despesa)
                .ToListAsync();
            viewModel.Assinaturas = await _contexto.Assinaturas.Where(a => a.UsuarioId == usuarioId && a.Ativa).ToListAsync();
            viewModel.Tags = await _contexto.Tags.Where(t => t.UsuarioId == usuarioId).OrderBy(t => t.Nome).ToListAsync();
            
            return View(viewModel);
        }

        // GET: Transacao/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var usuarioId = GetCurrentUserId();
            var transacao = await _contexto.Transacoes
                .Include(t => t.Conta)
                .Include(t => t.Categoria)
                .Where(t => t.Id == id && t.Conta.Usuario.Id == usuarioId)
                .FirstOrDefaultAsync();

            if (transacao == null) return NotFound();
            return View(transacao);
        }

        // POST: Transacao/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarioId = GetCurrentUserId();
            var transacao = await _contexto.Transacoes
                .Include(t => t.Conta)
                .Where(t => t.Id == id && t.Conta.Usuario.Id == usuarioId)
                .FirstOrDefaultAsync();

            if (transacao != null)
            {
                _contexto.Transacoes.Remove(transacao);
                await _contexto.SaveChangesAsync();
                TempData["Success"] = "Transação excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}