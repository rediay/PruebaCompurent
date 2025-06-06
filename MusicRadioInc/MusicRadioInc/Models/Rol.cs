using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MusicRadioInc.Models
{
    public class Rol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del rol no debe sobrepasar los 50 caracteres.")]
        public string? NombreRol { get; set; }

        // Propiedad de navegación para la relación con Usuarios
        // Una forma simple de muchos a muchos (a través de una tabla de unión)
        // O una relación uno a muchos simple si cada usuario tiene un solo rol
        // Para simplificar, asumiremos una relación uno a muchos donde cada Usuario tiene un solo Rol por ahora.
        // Si necesitas muchos a muchos, se complica un poco más con una tabla de unión.
        public ICollection<Client> Clients { get; set; }
    }
}
