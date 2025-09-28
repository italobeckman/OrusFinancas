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

                    if (conta != null)
                    {
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
            public async Task<bool> GerarDespesaAssinatura(int assinaturaId)
            {
                var assinatura = await _contexto.Assinaturas
                    .Include(a => a.Usuario)
                    .FirstOrDefaultAsync(a => a.Id == assinaturaId && a.Ativa);

                if (assinatura == null) return false;

                var hoje = DateTime.Today;

                // Verifica se j� existe transa��o para hoje
                var transacaoExistente = await _contexto.Despesas
                    .AnyAsync(d => d.AssinaturaId == assinaturaId &&
                                   d.DataTransacao.Date == hoje);

                if (transacaoExistente) return false;

                // Buscar ou criar categoria de assinaturas
                var categoria = await _contexto.Categorias
                    .FirstOrDefaultAsync(c => c.UsuarioId == assinatura.UsuarioId &&
                                             c.Nome.ToLower().Contains("assinatura"));

                if (categoria == null)
                {
                    categoria = new Categoria
                    {
                        Nome = "Assinaturas",
                        TipoCategoria = TipoCategoria.Despesa,
                        UsuarioId = assinatura.UsuarioId
                    };
                    _contexto.Categorias.Add(categoria);
                    await _contexto.SaveChangesAsync();
                }

                // --- L�gica Corrigida para a Conta ---
                // Primeiro, verifique se a assinatura tem uma conta espec�fica.
                int? contaId = assinatura.ContaId;

                // Se a assinatura n�o tem uma conta espec�fica, encontre ou crie uma conta padr�o.
                if (!contaId.HasValue)
                {
                    var contaPadrao = await _contexto.Contas
                        .Where(c => c.UsuarioId == assinatura.UsuarioId)
                        .FirstOrDefaultAsync();

                    // Se o usu�rio n�o tiver nenhuma conta, crie uma "Conta Padr�o"
                    if (contaPadrao == null)
                    {
                        contaPadrao = new Conta
                        {
                            NomeBanco = Bancos.ContaPadrao,
                            UsuarioId = assinatura.UsuarioId
                        };
                        _contexto.Contas.Add(contaPadrao);
                        await _contexto.SaveChangesAsync();
                    }
                    contaId = contaPadrao.Id;
                }

                // Se ainda n�o tivermos um ID de conta v�lido, algo est� errado.
                if (!contaId.HasValue) return false;

                // Criar despesa
                var despesa = new Despesa
                {
                    DataTransacao = hoje,
                    Valor = assinatura.ValorMensal,
                    Descricao = $"Assinatura - {assinatura.Servico}",
                    ContaId = contaId.Value, // Use o ID de conta v�lido aqui
                    CategoriaId = categoria.Id,
                    AssinaturaId = assinatura.Id
                };

                _contexto.Despesas.Add(despesa);
                await _contexto.SaveChangesAsync();

                return true;
            }
      

        private async Task<int> GetContaPadraoUsuario(int usuarioId)
        {
            var conta = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .FirstOrDefaultAsync();
                
            return conta?.Id ?? 0;
        }
    }
}