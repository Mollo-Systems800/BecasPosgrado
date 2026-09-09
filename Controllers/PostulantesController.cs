using BecasPosgrado.Data;
using BecasPosgrado.Filters;
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
                ViewBag.Error = ex.Message;
                return View(new List<Postulante>());
            }
        }

        public IActionResult Create() => View(new Postulante());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Postulante model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                _db.CrearPostulante(model);
                TempData["Mensaje"] = "Postulante creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, "Error de base de datos: " + ex.Message);
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var p = _db.ObtenerPostulante(id);
                if (p == null) return NotFound();
                return View(p);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = ex.Message;
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
                _db.ActualizarPostulante(model);
                TempData["Mensaje"] = "Postulante actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, "Error de base de datos: " + ex.Message);
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
                TempData["Mensaje"] = "No se pudo eliminar: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
