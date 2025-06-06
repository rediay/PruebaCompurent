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
        public DbSet<SongSet> SongSets { get; set; }     
        public DbSet<AlbumSet> AlbumSets { get; set; }   
        public DbSet<PurchaseDetail> PurchaseDetails { get; set; } 
        public DbSet<Client> Clients { get; set; }       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración adicional para la tabla de usuarios si fuera necesario
            // Por ejemplo, para asegurar que UserLoginId sea único
            modelBuilder.Entity<Client>()
                .HasIndex(u => u.UserLoginId)
                .IsUnique();
            // Configuración de la relación uno a muchos entre Rol y Usuario
            modelBuilder.Entity<Client>()
                .HasOne(u => u.Rol) // Un Usuario tiene un Rol
                .WithMany(r => r.Clients) // Un Rol puede tener muchos Usuarios
                .HasForeignKey(u => u.RolId) // La clave foránea en Usuario es RolId
                .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada de roles si hay usuarios

            // Asegurar que el nombre del rol sea único
            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.NombreRol)
                .IsUnique();

            // AlbumSet
            modelBuilder.Entity<AlbumSet>()
                .Property(a => a.Price)
                .HasColumnType("decimal(18, 2)"); // Asegura el tipo decimal en DB

            // SongSet
            modelBuilder.Entity<SongSet>()
                .HasOne(s => s.Album) // Una canción tiene un álbum
                .WithMany(a => a.Songs) // Un álbum tiene muchas canciones
                .HasForeignKey(s => s.Album_Id)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina un álbum, sus canciones se eliminan

            // PurchaseDetail
            modelBuilder.Entity<PurchaseDetail>()
                .Property(pd => pd.Total)
                .HasColumnType("decimal(18, 2)"); // Asegura el tipo decimal en DB

            modelBuilder.Entity<PurchaseDetail>()
                .HasOne(pd => pd.Client) // Un detalle de compra tiene un cliente
                .WithMany(c => c.PurchaseDetails) // Un cliente tiene muchos detalles de compra
                .HasForeignKey(pd => pd.Client_Id)
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminar clientes si tienen compras

            modelBuilder.Entity<PurchaseDetail>()
                .HasOne(pd => pd.Album) // Un detalle de compra tiene un álbum
                .WithMany(a => a.PurchaseDetails) // Un álbum puede estar en muchos detalles de compra
                .HasForeignKey(pd => pd.Album_Id)
                .OnDelete(DeleteBehavior.Restrict); // Evitar eliminar álbumes si están en compras
        }
    }
}
