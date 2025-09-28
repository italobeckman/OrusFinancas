using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models; // Assumindo que suas classes de modelo estão aqui

namespace OrusFinancas.Models
{
    public class Contexto: DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {

        }
        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PerfilFinanceiro> PerfisFinanceiros { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Orcamento> Orcamentos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Assinatura> Assinaturas { get; set; }
        public DbSet<InsightFinanceiro> Insights { get; set; }
        
        public DbSet<Transacao> Transacoes { get; set; } 
        public DbSet<Receita> Receitas { get; set; }
        public DbSet<Despesa> Despesas { get; set; }

        // ADICIONE ESTE MÉTODO PARA CONFIGURAR AS RELAÇÕES
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Resolver o erro de "multiple cascade paths" na relação Conta -> Transacao.
            // Isso evita que o banco de dados entre em conflito se houver outras FKs com cascade.
            modelBuilder.Entity<Transacao>()
                .HasOne(t => t.Conta)
                .WithMany(c => c.Transacoes) // Assumindo que você tem uma coleção 'Transacoes' em 'Conta'
                .HasForeignKey(t => t.ContaId) // Assumindo que a FK é 'ContaId'
                .OnDelete(DeleteBehavior.Restrict); // <--- A CHAVE DA SOLUÇÃO

            // Configurar categoria como opcional para receitas
            modelBuilder.Entity<Transacao>()
                .HasOne(t => t.Categoria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull); // Permite nulo

            // 2. (Opcional, mas recomendado) Se a exclusão de um Usuário pode causar mais problemas:
            // modelBuilder.Entity<Conta>()
            //     .HasOne(c => c.Usuario)
            //     .WithMany(u => u.Contas) // Assumindo que você tem uma coleção 'Contas' em 'Usuario'
            //     .OnDelete(DeleteBehavior.Restrict);

            // Chamada obrigatória para a classe base

            modelBuilder.Entity<Assinatura>()
            .HasOne(a => a.Usuario) // A Assinatura TEM uma propriedade de navegação 'Usuario'
            .WithMany() // O Usuario tem MUITAS Assinaturas, mas NÃO há uma propriedade de coleção na classe Usuario.
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict); // MANTENDO a restrição para evitar o erro de cascata!

            // Configurar relacionamento de assinatura com transação como opcional
            modelBuilder.Entity<Transacao>()
                .HasOne(t => t.Assinatura)
                .WithMany(a => a.TransacoesGeradas)
                .HasForeignKey(t => t.AssinaturaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar tipos decimal para evitar warnings
            modelBuilder.Entity<Assinatura>()
                .Property(a => a.ValorMensal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Orcamento>()
                .Property(o => o.ValorLimite)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<PerfilFinanceiro>()
                .Property(p => p.RendaMensal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transacao>()
                .Property(t => t.Valor)
                .HasColumnType("decimal(18,2)");

            // Configurar herança TPH (Table Per Hierarchy)
            modelBuilder.Entity<Transacao>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Receita>("Receita")
                .HasValue<Despesa>("Despesa");

            base.OnModelCreating(modelBuilder);
        }
    }
}