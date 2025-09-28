using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Models.ViewModels;
using System.Security.Claims;

namespace OrusFinancas.Controllers
{
    [Authorize]
    public class RelatorioController : Controller
    {
        private readonly Contexto _contexto;

        public RelatorioController(Contexto contexto)
        {
            _contexto = contexto;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }

        // Extrato Financeiro
        public async Task<IActionResult> Extrato(int? contaId = null, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            var usuarioId = GetCurrentUserId();

            if (!dataInicio.HasValue)
                dataInicio = DateTime.Today.AddMonths(-1);
            
            if (!dataFim.HasValue)
                dataFim = DateTime.Today;

            var query = _contexto.Transacoes
                .Include(t => t.Conta)
                .Include(t => t.Categoria)
                .Where(t => t.Conta.UsuarioId == usuarioId &&
                           t.DataTransacao >= dataInicio &&
                           t.DataTransacao <= dataFim);

            if (contaId.HasValue)
            {
                query = query.Where(t => t.ContaId == contaId);
            }

            var transacoes = await query
                .OrderByDescending(t => t.DataTransacao)
                .ToListAsync();

            // Dados para o filtro
            var contas = await _contexto.Contas
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            ViewBag.Contas = contas;
            ViewBag.ContaId = contaId;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

            return View(transacoes);
        }

        // Gráfico de Gastos por Categoria
        public async Task<IActionResult> GraficoGastosPorCategoria(int mes = 0, int ano = 0)
        {
            var usuarioId = GetCurrentUserId();

            if (mes == 0) mes = DateTime.Now.Month;
            if (ano == 0) ano = DateTime.Now.Year;

            var gastosPorCategoria = await _contexto.Despesas
                .Include(d => d.Categoria)
                .Include(d => d.Conta)
                .Where(d => d.Conta.UsuarioId == usuarioId &&
                           d.DataTransacao.Month == mes &&
                           d.DataTransacao.Year == ano)
                .GroupBy(d => new { d.Categoria.Id, d.Categoria.Nome })
                .Select(g => new GastosPorCategoriaViewModel
                {
                    Categoria = g.Key.Nome,
                    Total = g.Sum(d => d.Valor)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            ViewBag.Mes = mes;
            ViewBag.Ano = ano;
            ViewBag.MesNome = new DateTime(ano, mes, 1).ToString("MMMM yyyy");

            return View(gastosPorCategoria);
        }

        // Balanço Mensal
        public async Task<IActionResult> BalancoMensal(int mes = 0, int ano = 0)
        {
            var usuarioId = GetCurrentUserId();

            if (mes == 0) mes = DateTime.Now.Month;
            if (ano == 0) ano = DateTime.Now.Year;

            var receitas = await _contexto.Receitas
                .Include(r => r.Conta)
                .Where(r => r.Conta.UsuarioId == usuarioId &&
                           r.DataTransacao.Month == mes &&
                           r.DataTransacao.Year == ano)
                .SumAsync(r => (decimal?)r.Valor) ?? 0m;

            var despesas = await _contexto.Despesas
                .Include(d => d.Conta)
                .Where(d => d.Conta.UsuarioId == usuarioId &&
                           d.DataTransacao.Month == mes &&
                           d.DataTransacao.Year == ano)
                .SumAsync(d => (decimal?)d.Valor) ?? 0m;

            var receitasPorCategoria = await _contexto.Receitas
                .Include(r => r.Categoria)
                .Include(r => r.Conta)
                .Where(r => r.Conta.UsuarioId == usuarioId &&
                           r.DataTransacao.Month == mes &&
                           r.DataTransacao.Year == ano)
                .GroupBy(r => r.Categoria.Nome)
                .Select(g => new 
                {
                    Categoria = g.Key,
                    Total = g.Sum(r => r.Valor)
                })
                .ToListAsync();

            var despesasPorCategoria = await _contexto.Despesas
                .Include(d => d.Categoria)
                .Include(d => d.Conta)
                .Where(d => d.Conta.UsuarioId == usuarioId &&
                           d.DataTransacao.Month == mes &&
                           d.DataTransacao.Year == ano)
                .GroupBy(d => d.Categoria.Nome)
                .Select(g => new 
                {
                    Categoria = g.Key,
                    Total = g.Sum(d => d.Valor)
                })
                .ToListAsync();

            var model = new
            {
                Mes = mes,
                Ano = ano,
                MesNome = new DateTime(ano, mes, 1).ToString("MMMM yyyy"),
                TotalReceitas = receitas,
                TotalDespesas = despesas,
                Saldo = receitas - despesas,
                ReceitasPorCategoria = receitasPorCategoria,
                DespesasPorCategoria = despesasPorCategoria
            };

            return View(model);
        }

        // Relatório de Assinaturas
        public async Task<IActionResult> RelatorioAssinaturas()
        {
            var usuarioId = GetCurrentUserId();

            var assinaturasAtivas = await _contexto.Assinaturas
                .Where(a => a.UsuarioId == usuarioId && a.Ativa)
                .ToListAsync();

            var totalMensal = assinaturasAtivas.Sum(a => a.ValorMensal);
            var totalAnual = totalMensal * 12;

            // Gastos com assinaturas no último ano
            var dataLimite = DateTime.Today.AddYears(-1);
            var gastosAssinaturas = await _contexto.Despesas
                .Include(d => d.Conta)
                .Include(d => d.Categoria)
                .Where(d => d.Conta.UsuarioId == usuarioId &&
                           d.DataTransacao >= dataLimite &&
                           d.AssinaturaId != null)
                .GroupBy(d => d.AssinaturaId)
                .Select(g => new 
                {
                    AssinaturaId = g.Key,
                    TotalGasto = g.Sum(d => d.Valor),
                    Quantidade = g.Count()
                })
                .ToListAsync();

            var model = new
            {
                AssinaturasAtivas = assinaturasAtivas,
                TotalMensal = totalMensal,
                TotalAnual = totalAnual,
                GastosAssinaturas = gastosAssinaturas
            };

            return View(model);
        }
    }
}