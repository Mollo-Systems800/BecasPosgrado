using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BecasPosgrado.Filters
{
    // Uso: [RequiereRol("ADMIN")]. Verifica primero que haya sesión iniciada
    // y luego que el rol guardado en Session coincida con el requerido.
    public class RequiereRolAttribute : ActionFilterAttribute
    {
        private readonly string _rol;

        public RequiereRolAttribute(string rol)
        {
            _rol = rol;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var idPostulante = context.HttpContext.Session.GetInt32("IdPostulante");
            if (idPostulante == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var rolSesion = context.HttpContext.Session.GetString("Rol");
            if (!string.Equals(rolSesion, _rol, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
