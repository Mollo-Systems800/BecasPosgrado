using BecasPosgrado.Data;
using BecasPosgrado.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    public class AccountController : Controller
    {
        private readonly OracleDbContext _db;

        public AccountController(OracleDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("IdPostulante") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var postulante = _db.ObtenerPostulantePorUsuario(model.Usuario, model.Clave);
                if (postulante == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                HttpContext.Session.SetInt32("IdPostulante", postulante.Id);
                HttpContext.Session.SetString("NombreCompleto", postulante.NombreCompleto);
                HttpContext.Session.SetString("Rol", postulante.Rol);

                return RedirectToAction("Index", "Home");
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, "Error de base de datos: " + ex.Message);
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
