using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;

namespace OrusFinancas.Services
{
    public class AssinaturaService
    {
        private readonly Contexto _contexto;

        public AssinaturaService(Contexto contexto)
        {
            _contexto = contexto;
        }

        /// <summary>
        /// Gera transa��es autom�ticas para assinaturas vencidas
        /// </summary>
        public async Task GerarTransacoesAssinaturasAsync()
        {
            var hoje = DateTime.Today;
            
            // Buscar assinaturas ativas que venceram hoje
            var assinaturasVencidas = (await _contexto.Assinaturas
                .Include(a => a.Usuario)
                .Where(a => a.Ativa)
                .ToListAsync())
                .Where(a => a.ProximoVencimento <= hoje)
                .ToList();

            foreach (var assinatura in assinaturasVencidas)
            {
                // Verificar se j� foi gerada uma transa��o hoje para esta assinatura
                var transacaoExistente = await _contexto.Despesas
                    .AnyAsync(d => d.AssinaturaId == assinatura.Id && 
                                  d.DataTransacao.Date == hoje);

                if (!transacaoExistente)
                {
                    // Buscar conta padr�o do usu�rio ou a conta especificada na assinatura
                    var conta = assinatura.ContaId.HasValue ? 
                        await _contexto.Contas.FindAsync(assinatura.ContaId.Value) :
                        await _contexto.Contas.Where(c => c.UsuarioId == assinatura.UsuarioId).FirstOrDefaultAsync();

                    // Se n�o houver conta, pular esta assinatura
                    if (conta == null) continue;

                    // Buscar categoria "Assinaturas" ou criar uma gen�rica
                    var categoriaAssinatura = await _contexto.Categorias
                        .FirstOrDefaultAsync(c => c.UsuarioId == assinatura.UsuarioId && 
                                                 (c.Nome.ToLower().Contains("assinatura") || 
                                                  c.Nome.ToLower().Contains("recorrente")));

                    if (categoriaAssinatura == null)
                    {
                        categoriaAssinatura = new Categoria
                        {
                            Nome = "Assinaturas",
                            TipoCategoria = TipoCategoria.Despesa,
                            UsuarioId = assinatura.UsuarioId
                        };
                        _contexto.Categorias.Add(categoriaAssinatura);
                        await _contexto.SaveChangesAsync();
                    }

                    // Criar despesa autom�tica
                    var despesa = new Despesa
                    {
                        DataTransacao = hoje,
                        Valor = assinatura.ValorMensal,
                        Descricao = $"Assinatura - {assinatura.Servico} (Autom�tico)",
                        ContaId = conta.Id,
                        CategoriaId = categoriaAssinatura.Id,
                        AssinaturaId = assinatura.Id
                    };

                    _contexto.Despesas.Add(despesa);
                }
            }

            await _contexto.SaveChangesAsync();
        }

        /// <summary>
        /// Obt�m assinaturas pr�ximas ao vencimento
        /// </summary>
        public async Task<List<Assinatura>> GetAssinaturesProximasVencimentoAsync(int usuarioId, int diasAntecedencia = 5)
        {
            var dataLimite = DateTime.Today.AddDays(diasAntecedencia);
            
            return await _contexto.Assinaturas
                .Where(a => a.UsuarioId == usuarioId && 
                           a.Ativa && 
                           a.ProximoVencimento <= dataLimite)
                .OrderBy(a => a.ProximoVencimento)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula o total mensal de assinaturas para um usu�rio
        /// </summary>
        public async Task<decimal> GetTotalMensalAssinaturasAsync(int usuarioId)
        {
            return await _contexto.Assinaturas
                .Where(a => a.UsuarioId == usuarioId && a.Ativa)
                .SumAsync(a => a.ValorMensal);
        }

        /// <summary>
        /// Gerar despesas manualmente para uma assinatura espec�fica
        /// </summary>
        /// <param name="assinaturaId">ID da assinatura</param>
        /// <param name="usuarioId">ID do usu�rio logado (para valida��o de seguran�a)</param>
        public async Task<bool> GerarDespesaAssinatura(int assinaturaId, int usuarioId)
        {
            // CORRE��O PRINCIPAL: Validar que a assinatura pertence ao usu�rio logado
            var assinatura = await _contexto.Assinaturas
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == assinaturaId && 
                                         a.UsuarioId == usuarioId && 
                                         a.Ativa);

            if (assinatura == null) return false;

            var hoje = DateTime.Today;

            // Verifica se j� existe transa��o para hoje
            var transacaoExistente = await _contexto.Despesas
                .AnyAsync(d => d.AssinaturaId == assinaturaId &&
                               d.DataTransacao.Date == hoje);

            if (transacaoExistente) return false;

            // Buscar ou criar categoria de assinaturas
            var categoria = await _contexto.Categorias
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId &&
                                         c.Nome.ToLower().Contains("assinatura"));

            if (categoria == null)
            {
                categoria = new Categoria
                {
                    Nome = "Assinaturas",
                    TipoCategoria = TipoCategoria.Despesa,
                    UsuarioId = usuarioId
                };
                _contexto.Categorias.Add(categoria);
                await _contexto.SaveChangesAsync();
            }

            // Buscar conta
            Conta? conta = null;
            
            if (assinatura.ContaId.HasValue)
            {
                conta = await _contexto.Contas
                    .FirstOrDefaultAsync(c => c.Id == assinatura.ContaId.Value && 
                                             c.UsuarioId == usuarioId);
            }
            
            // Se n�o h� conta especificada ou ela n�o pertence ao usu�rio, buscar qualquer conta do usu�rio
            if (conta == null)
            {
                conta = await _contexto.Contas
                    .Where(c => c.UsuarioId == usuarioId)
                    .FirstOrDefaultAsync();
            }

            // Se o usu�rio n�o tem conta, n�o pode gerar a despesa
            if (conta == null)
            {
                return false;
            }

            // Criar despesa
            var despesa = new Despesa
            {
                DataTransacao = hoje,
                Valor = assinatura.ValorMensal,
                Descricao = $"Assinatura - {assinatura.Servico}",
                ContaId = conta.Id,
                CategoriaId = categoria.Id,
                AssinaturaId = assinatura.Id
            };

            _contexto.Despesas.Add(despesa);
            
            try
            {
                await _contexto.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}