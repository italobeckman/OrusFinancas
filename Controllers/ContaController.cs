using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;

// [Authorize] // Adicione isso para garantir que apenas usuários logados acessem
public class ContaController : Controller
{
    private readonly Contexto _contexto;

    public ContaController(Contexto contexto)
    {
        _contexto = contexto;
    }

    // Ação para exibir a lista de contas (READ - Listagem)
    public async Task<IActionResult> Index()
    {

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
        {
            // Se o usuário não for válido, adicione o erro e retorne uma View vazia.
            ModelState.AddModelError(string.Empty, "Erro de autenticação. Por favor, faça o login novamente.");
            return View(new List<Conta>()); // Retorna uma lista vazia para a View não quebrar.
        }
        // 1. Busca as contas do usuário
        var contas = await _contexto.Contas
            .Where(c => c.Usuario.Id == usuarioId)
            .ToListAsync();
            
        // 2. Cálculo do Saldo Atual (Regra de Negócio)
        // Isso é complexo e deve ser feito em um serviço, mas faremos aqui por simplicidade.
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
        // 1. O SERVIDOR determina a identidade do usuário (Seguro)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
        {
            ModelState.AddModelError(string.Empty, "Erro de autenticação.");
            return View(conta);
        }

        // 2. O SERVIDOR atribui a identidade ao objeto que veio do formulário
        var usuarioDoBanco = await _contexto.Usuarios.FindAsync(usuarioId);

        // 2. Verifique se o usuário foi realmente encontrado
        if (usuarioDoBanco == null)
        {
            ModelState.AddModelError(string.Empty, "Erro! Usuário não encontrado.");
            return View(conta);
        }
        else
        {
            conta.Usuario = usuarioDoBanco;
        }

        // 3. O SERVIDOR informa ao ModelState: "Eu cuidei desses campos, pode ignorar
        //    qualquer erro de validação que você encontrou para eles."
        ModelState.Remove("Usuario");   // Objeto de navegação não vem do form
        // 4. Agora, o IsValid verifica APENAS os campos que o usuário preencheu (NomeBanco, etc.)
        if (ModelState.IsValid)
        {
            _contexto.Add(conta);
            await _contexto.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        
        
        // Se ainda for inválido, é porque o usuário esqueceu de preencher o NomeBanco ou outro campo do formulário
        return View(conta);
    }
        
    // As ações Edit (Atualizar) e Delete (Deletar) seguem o mesmo padrão
    // de busca (FirstOrDefaultAsync), verificação de UserID e manipulação do DBContext.
}