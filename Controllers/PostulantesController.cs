using BecasPosgrado.Data;
using BecasPosgrado.Filters;
using BecasPosgrado.Helpers;
using BecasPosgrado.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    [RequiereRol("ADMIN")]
    public class PostulantesController : Controller
    {
        private readonly OracleDbContext _db;

        public PostulantesController(OracleDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            try
            {
                return View(_db.ListarPostulantes());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return View(new List<Postulante>());
            }
        }

        public IActionResult Create() => View(new Postulante());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Postulante model)
        {
            if (string.IsNullOrWhiteSpace(model.Clave))
            {
                ModelState.AddModelError(nameof(model.Clave), "La contraseña es obligatoria.");
            }
            if (!ModelState.IsValid) return View(model);
            try
            {
                _db.CrearPostulante(model);
                TempData["Mensaje"] = "Postulante creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, OracleErrorHelper.MensajeAmigable(ex));
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var p = _db.ObtenerPostulante(id);
                if (p == null) return NotFound();
                p.Clave = string.Empty;
                return View(p);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Postulante model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                bool cambiarClave = !string.IsNullOrWhiteSpace(model.Clave);
                _db.ActualizarPostulante(model, cambiarClave);
                TempData["Mensaje"] = "Postulante actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, OracleErrorHelper.MensajeAmigable(ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.EliminarPostulante(id);
                TempData["Mensaje"] = "Postulante eliminado correctamente.";
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "No se pudo eliminar: " + OracleErrorHelper.MensajeAmigable(ex);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
