using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using OrusFinancas.Utils;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    public class AuthController : Controller
    {
        private readonly Contexto _contexto;

        public AuthController(Contexto contexto)
        {
            _contexto = contexto;
        }

        // GET: /Auth/Welcome
        public IActionResult Welcome()
        {
            // Se o usuário já está autenticado, redireciona para o dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            // Se o usuário já está autenticado, redireciona para o dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        // POST: /Auth/Login
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
                            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                            new Claim(ClaimTypes.Name, usuario.Nome),
                            new Claim(ClaimTypes.Email, usuario.Email)
                        };

                        var identity = new ClaimsIdentity(claims, "Cookies");
                        var principal = new ClaimsPrincipal(identity);

                        // 4. Autenticar (Gerar o cookie de sessão)
                        await HttpContext.SignInAsync("Cookies", principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = model.LembrarMe
                            });

                        // 5. Redirecionar após o sucesso
                        return RedirectToAction("Index", "Home");
                    }
                }

                // Se o usuário não foi encontrado ou a senha está incorreta
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            }

            // Se a validação falhar, retorna o formulário
            return View(model);
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            // Se o usuário já está autenticado, redireciona para o dashboard
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UsuarioCadastroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar se o email já existe
            var emailExistente = await _contexto.Usuarios
                .AnyAsync(u => u.Email == model.Email);

            if (emailExistente)
            {
                ModelState.AddModelError("Email", "Este e-mail já está em uso.");
                return View(model);
            }

            // 1. Gerar o Hash da senha
            string hashedPassword = PasswordHasher.HashPassword(model.SenhaPura);

            // 2. Criar a entidade de usuário
            var usuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                SenhaHash = hashedPassword,
                DataCadastro = DateTime.UtcNow
            };

            // 3. Salvar no banco
            _contexto.Add(usuario);
            await _contexto.SaveChangesAsync();

            // 4. Fazer login automático após o cadastro
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            // 5. Redirecionar para o dashboard
            TempData["Success"] = "Conta criada com sucesso! Bem-vindo ao Órus Finanças!";
            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Welcome");
        }

        // GET: /Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}