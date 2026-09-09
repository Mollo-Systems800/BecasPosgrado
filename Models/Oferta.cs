using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla OFERTA
    public class Oferta : IValidatableObject
    {
        public int Id { get; set; }

        [Required]
        public int IdPrograma { get; set; }

        [Required]
        public int IdSede { get; set; }

        [Required(ErrorMessage = "Fecha límite obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaLimitePostulacion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Los cupos no pueden ser negativos.")]
        public int CuposDisponibles { get; set; }

        // 'Activa' o 'Cerrada'
        public string Estado { get; set; } = "Activa";

        // Campos solo de lectura, llenados por los JOIN de ListarOfertasActivas /
        // ConsultarOfertasConFiltro / ListarTodasOfertas para mostrar en las vistas.
        public string? ProgramaNombre { get; set; }
        public string? Area { get; set; }
        public string? UniversidadNombre { get; set; }
        public string? SedeNombre { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin <= FechaInicio)
            {
                yield return new ValidationResult(
                    "Debe ser posterior al inicio.",
                    new[] { nameof(FechaFin) });
            }

            if (FechaLimitePostulacion > FechaFin)
            {
                yield return new ValidationResult(
                    "Debe ser igual o anterior al fin.",
                    new[] { nameof(FechaLimitePostulacion) });
            }
        }
    }
}
