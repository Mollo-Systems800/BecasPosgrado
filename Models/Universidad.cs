using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla UNIVERSIDAD
    public class Universidad
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [RegularExpression(@"^(?=.*\p{L})[\p{L}\p{N} .,'&\-]+$", ErrorMessage = "Debe contener letras.")]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [RegularExpression(@"^[\p{L} .\-]+$", ErrorMessage = "Solo letras.")]
        public string Pais { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [RegularExpression(@"^[\p{L} .\-]+$", ErrorMessage = "Solo letras.")]
        public string Ciudad { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }
    }
}
