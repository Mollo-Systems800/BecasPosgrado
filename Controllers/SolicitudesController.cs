using BecasPosgrado.Data;
using BecasPosgrado.Filters;
using BecasPosgrado.Helpers;
using BecasPosgrado.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    [RequiereSesion]
    public class SolicitudesController : Controller
    {
        private readonly OracleDbContext _db;

        public SolicitudesController(OracleDbContext db)
        {
            _db = db;
        }

        // ADMIN ve todas las solicitudes; POSTULANTE ve solo las suyas.
        public IActionResult Index()
        {
            var rol = HttpContext.Session.GetString("Rol");
            ViewBag.Rol = rol;

            try
            {
                List<Solicitud> lista;
                if (rol == "ADMIN")
                {
                    lista = _db.ListarTodasSolicitudes();
                }
                else
                {
                    var idPostulante = HttpContext.Session.GetInt32("IdPostulante")!.Value;
                    lista = _db.ListarSolicitudesPorPostulante(idPostulante);
                }
                return View(lista);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return View(new List<Solicitud>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiereRol("ADMIN")]
        public IActionResult Aceptar(int id)
        {
            try
            {
                // Llama a F_ACEPTAR_SOLICITUD y muestra el mensaje que devuelve.
                var mensaje = _db.AceptarSolicitud(id);
                TempData["Mensaje"] = mensaje;
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "Error: " + OracleErrorHelper.MensajeAmigable(ex);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiereRol("ADMIN")]
        public IActionResult Rechazar(int id)
        {
            try
            {
                // Llama a F_RECHAZAR_SOLICITUD y muestra el mensaje que devuelve.
                var mensaje = _db.RechazarSolicitud(id);
                TempData["Mensaje"] = mensaje;
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "Error: " + OracleErrorHelper.MensajeAmigable(ex);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
