using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BecasPosgrado.Filters
{
    // Reemplaza a [Authorize] porque el login se maneja con Session, no con
    // ASP.NET Identity / cookies de autenticación.
    public class RequiereSesionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var idPostulante = context.HttpContext.Session.GetInt32("IdPostulante");
            if (idPostulante == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
