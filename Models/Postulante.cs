using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla POSTULANTE
    public class Postulante : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Obligatorio."), StringLength(100)]
        [RegularExpression(@"^[\p{L} .\-']+$", ErrorMessage = "Solo letras.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Obligatorio."), StringLength(100)]
        [RegularExpression(@"^[\p{L} .\-']+$", ErrorMessage = "Solo letras.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "Obligatorio."), StringLength(100)]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [RegularExpression(@"^[0-9+\-\s()]{6,20}$", ErrorMessage = "Teléfono inválido.")]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "Obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaNac { get; set; }

        [Required, StringLength(50)]
        public string Usuario { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Clave { get; set; } = string.Empty;

        // 'ADMIN' o 'POSTULANTE'
        public string Rol { get; set; } = "POSTULANTE";

        // ---------- Datos académicos (formación previa con la que postula) ----------

        [StringLength(50)]
        public string? NivelAcademico { get; set; }

        [StringLength(150)]
        [RegularExpression(@"^[\p{L}\p{N} .,'&\-]+$", ErrorMessage = "Solo letras y números.")]
        public string? InstitucionProcedencia { get; set; }

        [StringLength(150)]
        [RegularExpression(@"^[\p{L}\p{N} .,'&\-]+$", ErrorMessage = "Solo letras y números.")]
        public string? TituloObtenido { get; set; }

        [Range(0, 100, ErrorMessage = "Entre 0 y 100.")]
        public decimal? PromedioAcademico { get; set; }

        // ---------- Datos laborales ----------

        [StringLength(150)]
        [RegularExpression(@"^[\p{L}\p{N} .,'&\-]*$", ErrorMessage = "Solo letras y números.")]
        public string? EmpresaActual { get; set; }

        [StringLength(100)]
        [RegularExpression(@"^[\p{L} .\-]*$", ErrorMessage = "Solo letras.")]
        public string? CargoActual { get; set; }

        [Range(0, 80, ErrorMessage = "Entre 0 y 80.")]
        public int AniosExperiencia { get; set; } = 0;

        public string NombreCompleto => $"{Nombre} {Apellido}";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaNac.Date > DateTime.Today)
            {
                yield return new ValidationResult(
                    "No puede ser futura.",
                    new[] { nameof(FechaNac) });
            }
            else
            {
                var edad = DateTime.Today.Year - FechaNac.Year;
                if (FechaNac.Date > DateTime.Today.AddYears(-edad)) edad--;

                if (edad < 16)
                {
                    yield return new ValidationResult(
                        "Mínimo 16 años.",
                        new[] { nameof(FechaNac) });
                }
                else if (edad > 100)
                {
                    yield return new ValidationResult(
                        "Verifica la fecha.",
                        new[] { nameof(FechaNac) });
                }
            }
        }
    }
}
