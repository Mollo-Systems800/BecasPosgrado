using System.ComponentModel.DataAnnotations;

namespace BecasPosgrado.ViewModels
{
    public class PostularViewModel
    {
        [Required]
        public int IdOferta { get; set; }

        [Required(ErrorMessage = "Escribe un breve resumen de tu interés")]
        [Display(Name = "Resumen de interés")]
        public string ResumenInteres { get; set; } = string.Empty;
    }
}
