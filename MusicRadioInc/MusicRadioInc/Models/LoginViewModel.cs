using System.ComponentModel.DataAnnotations;

namespace MusicRadioInc.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
        [StringLength(10, ErrorMessage = "El ID de usuario no debe sobrepasar los 10 caracteres.")]
        [Display(Name = "ID de Usuario")]
        public string? UserLoginId { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(30, ErrorMessage = "La contraseña no debe sobrepasar los 30 caracteres.")]
        [Display(Name = "Contraseña")]
        public string? Password { get; set; }
    }
}
