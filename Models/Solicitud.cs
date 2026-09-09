namespace BecasPosgrado.Models
{
    // Coincide con la tabla SOLICITUD
    public class Solicitud
    {
        public int Id { get; set; }
        public int IdPostulante { get; set; }
        public int IdOferta { get; set; }
        public DateTime FechaPostulacion { get; set; }
        public string ResumenInteres { get; set; } = string.Empty;

        // 'Pendiente', 'Aceptada' o 'Rechazada'
        public string Estado { get; set; } = "Pendiente";

        // Campos solo de lectura para los listados (join con POSTULANTE / OFERTA-PROGRAMA)
        public string? PostulanteNombreCompleto { get; set; }
        public string? ProgramaNombre { get; set; }
        public string? UniversidadNombre { get; set; }
    }
}
