using BecasPosgrado.Data;
using BecasPosgrado.Filters;
using BecasPosgrado.Helpers;
using BecasPosgrado.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    [RequiereRol("ADMIN")]
    public class UniversidadesController : Controller
    {
        private readonly OracleDbContext _db;

        public UniversidadesController(OracleDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            try
            {
                return View(_db.ListarUniversidades());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return View(new List<Universidad>());
            }
        }

        public IActionResult Create() => View(new Universidad());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Universidad model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                _db.CrearUniversidad(model);
                TempData["Mensaje"] = "Universidad creada correctamente.";
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
                var u = _db.ObtenerUniversidad(id);
                if (u == null) return NotFound();
                return View(u);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Universidad model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                _db.ActualizarUniversidad(model);
                TempData["Mensaje"] = "Universidad actualizada correctamente.";
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
                _db.EliminarUniversidad(id);
                TempData["Mensaje"] = "Universidad eliminada correctamente.";
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "No se pudo eliminar: " + OracleErrorHelper.MensajeAmigable(ex);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
