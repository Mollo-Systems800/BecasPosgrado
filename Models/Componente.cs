using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla COMPONENTE (módulos/contenidos que describen un programa)
    public class Componente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Obligatorio."), StringLength(150)]
        [RegularExpression(@"^[\p{L}\p{N} .,'&\-]+$", ErrorMessage = "Solo letras y números.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona un programa.")]
        public int IdPrograma { get; set; }

        // Solo para mostrar en listados (join), no se inserta directamente
        public string? ProgramaNombre { get; set; }
    }
}
