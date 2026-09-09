using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla PROGRAMA
    public class Programa
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        // CHECK (area IN ('Especialidad','Maestría','Doctorado'))
        [Required, StringLength(50)]
        public string Area { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TipoFinanciamiento { get; set; }
    }
}
