using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.Models
{
    // Coincide con la tabla OFERTA
    public class Oferta
    {
        public int Id { get; set; }

        [Required]
        public int IdPrograma { get; set; }

        [Required]
        public int IdSede { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public int CuposDisponibles { get; set; }

        // 'Activa' o 'Cerrada'
        public string Estado { get; set; } = "Activa";

        // Campos solo de lectura, llenados por los JOIN de ListarOfertasActivas /
        // ConsultarOfertasConFiltro / ListarTodasOfertas para mostrar en las vistas.
        public string? ProgramaNombre { get; set; }
        public string? Area { get; set; }
        public string? UniversidadNombre { get; set; }
        public string? SedeNombre { get; set; }
    }
}
