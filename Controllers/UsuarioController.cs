using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.Utils;
using System.Security.Claims;
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
        // Dentro de UsuarioController.cs

        // Dentro de UsuarioController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCadastroViewModel model) 
        {
            // Se as regras de validação (incluindo o [Compare] das senhas) falharem, volta para a tela.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Gerar o Hash (Usando BCrypt)
            string hashedPassword = OrusFinancas.Utils.PasswordHasher.HashPassword(model.SenhaPura); 
            
            // 2. Criar a Entidade de Banco de Dados
            var usuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                SenhaHash = hashedPassword, // Salva APENAS o HASH
                DataCadastro = DateTime.UtcNow
            };

            // 3. Salvar no Banco
            _contexto.Add(usuario);
            await _contexto.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
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

        public IActionResult Login()
        {
            return View(); // Procura Views/Usuario/Login.cshtml
        }

        // 6. LOGIN (Processa o Formulário)
        // POST: /Usuario/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Encontrar o usuário pelo email
                var usuario = await _contexto.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (usuario != null)
                {
                    // 2. Verificar a senha usando BCrypt
                    if (PasswordHasher.VerifyPassword(model.SenhaPura, usuario.SenhaHash))
                    {
                        // 3. Criar a identidade (Claims)
                        var claims = new List<Claim>
                        {
                            // Claim principal (Identificação única)
                            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()), 
                            // Nome para exibição
                            new Claim(ClaimTypes.Name, usuario.Nome), 
                            // Qualquer outro dado útil (ex: Role, Email)
                            new Claim(ClaimTypes.Email, usuario.Email)
                        };

                        var identity = new ClaimsIdentity(claims, "Cookies");
                        var principal = new ClaimsPrincipal(identity);

                        // 4. Autenticar (Gerar o cookie de sessão)
                        await HttpContext.SignInAsync("Cookies", principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = model.LembrarMe // Lembrar-me: true = cookie durável
                            });

                        // 5. Redirecionar após o sucesso
                        return RedirectToAction(nameof(Index), "Home"); // Redireciona para a Home
                    }
                }

                // Se o usuário não foi encontrado ou a senha está incorreta
                ModelState.AddModelError(string.Empty, "E-mail ou Senha inválidos.");
            }

            // Se a validação falhar, retorna o formulário
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction(nameof(Index), "Home");
        }
        
        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}