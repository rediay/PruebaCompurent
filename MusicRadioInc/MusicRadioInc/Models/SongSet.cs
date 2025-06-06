using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MusicRadioInc.Models
{
    public class SongSet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la canción es obligatorio.")]
        [StringLength(255, ErrorMessage = "El nombre de la canción no debe exceder los 255 caracteres.")] // Ajustar longitud si nvarchar(max) no es ideal
        public string Name { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "El álbum es obligatorio para la canción.")]
        public int Album_Id { get; set; }

        [ForeignKey("Album_Id")]
        public AlbumSet Album { get; set; } // Propiedad de navegación
    }
}
