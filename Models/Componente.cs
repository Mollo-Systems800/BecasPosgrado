namespace BecasPosgrado.Models
{
    // Coincide con la tabla COMPONENTE (disponible para una futura pantalla de ABM;
    // no forma parte de las tareas explícitas del enunciado, pero el modelo ya
    // queda listo por si se necesita).
    public class Componente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int IdPrograma { get; set; }
        public string? ProgramaNombre { get; set; }
    }
}
