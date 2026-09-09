using BecasPosgrado.Data;
using BecasPosgrado.Filters;
using BecasPosgrado.Models;
using BecasPosgrado.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    [RequiereSesion]
    public class OfertasController : Controller
    {
        private readonly OracleDbContext _db;

        public OfertasController(OracleDbContext db)
        {
            _db = db;
        }

        // Listado de ofertas activas, visible para POSTULANTE y ADMIN.
        // El botón "Postularse" solo se muestra si el rol en Session es POSTULANTE (ver la vista).
        public IActionResult Index()
        {
            try
            {
                return View(_db.ListarOfertasActivas());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Oferta>());
            }
        }

        // Llamado desde el modal "Postularse" en Ofertas/Index.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Postularse(PostularViewModel model)
        {
            if (HttpContext.Session.GetString("Rol") != "POSTULANTE")
            {
                return RedirectToAction("AccesoDenegado", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Debes escribir un resumen de interés antes de postular.";
                return RedirectToAction(nameof(Index));
            }

            var idPostulante = HttpContext.Session.GetInt32("IdPostulante")!.Value;

            try
            {
                // Llama a F_REGISTRAR_SOLICITUD y muestra el mensaje devuelto por PL/SQL.
                var mensaje = _db.RegistrarSolicitud(idPostulante, model.IdOferta, model.ResumenInteres);
                TempData["Mensaje"] = mensaje;
            }
            catch (OracleException ex)
            {
                // Ej: "Ya tiene 3 solicitudes activas", límites, etc.
                TempData["Mensaje"] = "Error: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------------- ABM de ofertas (solo ADMIN) ----------------

        [RequiereRol("ADMIN")]
        public IActionResult Administrar()
        {
            try
            {
                return View(_db.ListarTodasOfertas());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Oferta>());
            }
        }

        private void CargarCombos(int? programaSeleccionado = null, int? sedeSeleccionada = null)
        {
            ViewBag.Programas = new SelectList(_db.ListarProgramas(), "Id", "Nombre", programaSeleccionado);
            ViewBag.Sedes = new SelectList(_db.ListarSedes(), "Id", "Nombre", sedeSeleccionada);
        }

        [RequiereRol("ADMIN")]
        public IActionResult Crear()
        {
            CargarCombos();
            return View(new Oferta { FechaInicio = DateTime.Today, FechaFin = DateTime.Today.AddMonths(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiereRol("ADMIN")]
        public IActionResult Crear(Oferta model)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos(model.IdPrograma, model.IdSede);
                return View(model);
            }
            try
            {
                _db.CrearOferta(model);
                TempData["Mensaje"] = "Oferta creada correctamente.";
                return RedirectToAction(nameof(Administrar));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, "Error de base de datos: " + ex.Message);
                CargarCombos(model.IdPrograma, model.IdSede);
                return View(model);
            }
        }

        [RequiereRol("ADMIN")]
        public IActionResult Editar(int id)
        {
            try
            {
                var o = _db.ObtenerOferta(id);
                if (o == null) return NotFound();
                CargarCombos(o.IdPrograma, o.IdSede);
                return View(o);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = ex.Message;
                return RedirectToAction(nameof(Administrar));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiereRol("ADMIN")]
        public IActionResult Editar(Oferta model)
        {
            if (!ModelState.IsValid)
            {
                CargarCombos(model.IdPrograma, model.IdSede);
                return View(model);
            }
            try
            {
                _db.ActualizarOferta(model);
                TempData["Mensaje"] = "Oferta actualizada correctamente.";
                return RedirectToAction(nameof(Administrar));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, "Error de base de datos: " + ex.Message);
                CargarCombos(model.IdPrograma, model.IdSede);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiereRol("ADMIN")]
        public IActionResult Eliminar(int id)
        {
            try
            {
                _db.EliminarOferta(id);
                TempData["Mensaje"] = "Oferta eliminada correctamente.";
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "No se pudo eliminar: " + ex.Message;
            }
            return RedirectToAction(nameof(Administrar));
        }
    }
}
