using OrusFinancas.Models;
using Microsoft.EntityFrameworkCore;

namespace OrusFinancas.Services
{
    public class SeedDataService
    {
        private readonly Contexto _contexto;
        
        public SeedDataService(Contexto contexto)
        {
            _contexto = contexto;
        }
        
        public async Task CriarCategoriasBasicasAsync(int usuarioId)
        {
            // Verificar se o usu�rio j� tem categorias
            var temCategorias = await _contexto.Categorias
                .AnyAsync(c => c.UsuarioId == usuarioId);
                
            if (temCategorias) return;
            
            var categoriasBasicas = new List<Categoria>
            {
                new() { Nome = "Alimenta��o", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Transporte", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Moradia", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Lazer", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Sa�de", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Educa��o", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Roupas", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Servi�os", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa },
                new() { Nome = "Outros", UsuarioId = usuarioId, TipoCategoria = TipoCategoria.Despesa }
            };
            
            _contexto.Categorias.AddRange(categoriasBasicas);
            await _contexto.SaveChangesAsync();
        }
    }
}