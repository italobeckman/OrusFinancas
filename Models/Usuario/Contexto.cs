using Microsoft.EntityFrameworkCore;

namespace OrusFinancas.Models
{
    public class Contexto: DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {

        }
        
        public DbSet<Usuario> Usuarios { get; set; }
    }
}