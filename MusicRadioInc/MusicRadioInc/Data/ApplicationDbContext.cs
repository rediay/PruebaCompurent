using Microsoft.EntityFrameworkCore;
using MusicRadioInc.Models;

namespace MusicRadioInc.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Usuario> Usuarios { get; set; } // Representa la tabla de usuarios en la DB
        public DbSet<Rol> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración adicional para la tabla de usuarios si fuera necesario
            // Por ejemplo, para asegurar que UserLoginId sea único
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.UserLoginId)
                .IsUnique();
            // Configuración de la relación uno a muchos entre Rol y Usuario
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol) // Un Usuario tiene un Rol
                .WithMany(r => r.Usuarios) // Un Rol puede tener muchos Usuarios
                .HasForeignKey(u => u.RolId) // La clave foránea en Usuario es RolId
                .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada de roles si hay usuarios

            // Asegurar que el nombre del rol sea único
            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.NombreRol)
                .IsUnique();
        }
    }
}
