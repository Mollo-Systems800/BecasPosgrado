using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla POSTULANTE
    public class Postulante
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [Required]
        public DateTime FechaNac { get; set; }

        [Required, StringLength(50)]
        public string Usuario { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Clave { get; set; } = string.Empty;

        // 'ADMIN' o 'POSTULANTE'
        public string Rol { get; set; } = "POSTULANTE";

        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
