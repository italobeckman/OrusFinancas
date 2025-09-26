using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using System.Threading.Tasks;

namespace OrusFinancas.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly Contexto _contexto;

        public UsuarioController(Contexto contexto)
        {
            _contexto = contexto;
        }

        // 1. READ (Lista)
        // GET: /Usuario/Index ou /Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = await _contexto.Usuarios.ToListAsync();
            // Procura a View em Views/Usuario/Index.cshtml
            return View(usuarios);
        }

        // 2. READ (Detalhes)
        // GET: /Usuario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _contexto.Usuarios
                                         .FirstOrDefaultAsync(m => m.Id == id);
            
            if (usuario == null)
            {
                return NotFound();
            }

            // Procura a View em Views/Usuario/Details.cshtml
            return View(usuario);
        }

        // 3. CREATE (Exibe o Formulário)
        // GET: /Usuario/Create
        public IActionResult Create()
        {
            // Procura a View em Views/Usuario/Create.cshtml
            return View();
        }

        // 3. CREATE (Recebe o Formulário)
        // POST: /Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Essencial para segurança (CSRF)
        public async Task<IActionResult> Create([Bind("Nome,Email,SenhaHash")] Usuario usuario)
        {
            // O modelo (propriedades e Data Annotations) está válido?
            if (ModelState.IsValid)
            {
                // Prepara campos de controle antes de salvar
                usuario.DataCadastro = DateTime.UtcNow;

                _contexto.Add(usuario);
                await _contexto.SaveChangesAsync();
                
                // Redireciona para a listagem após o sucesso
                return RedirectToAction(nameof(Index));
            }
            
            // Se o modelo for inválido, retorna o usuário para o formulário com os dados preenchidos e erros
            return View(usuario);
        }

        // 4. UPDATE (Exibe o Formulário)
        // GET: /Usuario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _contexto.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            
            // Procura a View em Views/Usuario/Edit.cshtml
            return View(usuario);
        }

        // 4. UPDATE (Recebe o Formulário)
        // POST: /Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Email,SenhaHash,DataCadastro")] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Marca o objeto como modificado e salva
                    _contexto.Update(usuario);
                    await _contexto.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_contexto.Usuarios.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // 5. DELETE (Exibe a Confirmação)
        // GET: /Usuario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (usuario == null)
            {
                return NotFound();
            }

            // Procura a View em Views/Usuario/Delete.cshtml
            return View(usuario);
        }

        // 5. DELETE (Executa a Remoção)
        // POST: /Usuario/Delete/5
        [HttpPost, ActionName("Delete")] // Usa ActionName para o método POST
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _contexto.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _contexto.Usuarios.Remove(usuario);
            }

            await _contexto.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}