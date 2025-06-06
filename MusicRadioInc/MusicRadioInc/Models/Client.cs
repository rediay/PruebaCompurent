using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MusicRadioInc.Models
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Generado por la base de datos
        public int Id { get; set; } // Auto-incremental para uso interno de la DB, diferente al Id del usuario

        [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID de usuario no debe sobrepasar los 10 caracteres.")]
        [Display(Name = "ID de Usuario")]
        public string? UserLoginId { get; set; } // Este será el Id de usuario para el login

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(30, ErrorMessage = "La contraseña no debe sobrepasar los 30 caracteres.")]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }

        [StringLength(100, ErrorMessage = "El nombre no debe sobrepasar los 100 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(50, ErrorMessage = "El correo no debe sobrepasar los 50 caracteres.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [Display(Name = "Correo Electrónico")]
        public string? Mail { get; set; }

        [StringLength(500, ErrorMessage = "La dirección no debe sobrepasar los 500 caracteres.")]
        [Display(Name = "Dirección")]
        public string? Direction { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no debe sobrepasar los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }

        public int RolId { get; set; }

        [JsonIgnore]
        [ForeignKey("RolId")]
        public Rol? Rol { get; set; } // Propiedad de navegación
        public ICollection<PurchaseDetail>? PurchaseDetails { get; set; }
    }
}
