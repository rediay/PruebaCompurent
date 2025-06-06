using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MusicRadioInc.Models
{
    public class AlbumSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del álbum es obligatorio.")]
        [StringLength(255, ErrorMessage = "El nombre del álbum no debe exceder los 255 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El precio del álbum es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")] // Tipo de dato para valores monetarios
        [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Price { get; set; } // Agregamos un campo de precio para las compras

        // Propiedad de navegación para las canciones en este álbum
        public ICollection<SongSet> Songs { get; set; }

        // Propiedad de navegación para los detalles de compra que incluyen este álbum
        public ICollection<PurchaseDetail> PurchaseDetails { get; set; }
    }
}
