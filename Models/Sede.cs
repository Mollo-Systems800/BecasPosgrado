using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla SEDE
    public class Sede
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [RegularExpression(@"^(?=.*\p{L})[\p{L}\p{N} .,'&\-]+$", ErrorMessage = "Debe contener letras.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Direccion { get; set; }

        [Required]
        public int IdUniversidad { get; set; }

        // Solo para mostrar en listados (join), no se inserta directamente
        public string? UniversidadNombre { get; set; }
    }
}
