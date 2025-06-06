using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MusicRadioInc.Models
{
    public class PurchaseDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Identificador de la compra (autonumérico)

        [Required(ErrorMessage = "El cliente es obligatorio para el detalle de compra.")]
        [StringLength(10)] // Coincide con la longitud del Id de Cliente
        public int Client_Id { get; set; } // Foreign Key

        [ForeignKey("Client_Id")]
        public Client? Client { get; set; } // Propiedad de navegación

        [Required(ErrorMessage = "El álbum es obligatorio para el detalle de compra.")]
        public int Album_Id { get; set; } // Foreign Key

        [ForeignKey("Album_Id")]
        public AlbumSet? Album { get; set; } // Propiedad de navegación

        [Required(ErrorMessage = "El total de la compra es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")] // Tipo de dato para valores monetarios
        public decimal Total { get; set; } // Valor de la compra (tipo real)

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now; // Fecha de la compra, con valor por defecto
    }
}
