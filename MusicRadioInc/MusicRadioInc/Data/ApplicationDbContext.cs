using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Models;

namespace MusicRadioInc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } // Representa la tabla de usuarios en la DB

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración adicional para la tabla de usuarios si fuera necesario
            // Por ejemplo, para asegurar que UserLoginId sea único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.UserLoginId)
                .IsUnique();
        }
    }
}
