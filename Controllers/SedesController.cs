using BecasPosgrado.Data;
using BecasPosgrado.Filters;
using BecasPosgrado.Helpers;
using BecasPosgrado.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    [RequiereRol("ADMIN")]
    public class SedesController : Controller
    {
        private readonly OracleDbContext _db;

        public SedesController(OracleDbContext db)
        {
            _db = db;
        }

        private void CargarUniversidades(int? seleccionada = null)
        {
            ViewBag.Universidades = new SelectList(_db.ListarUniversidades(), "Id", "Nombre", seleccionada);
        }

        public IActionResult Index()
        {
            try
            {
                return View(_db.ListarSedes());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return View(new List<Sede>());
            }
        }

        public IActionResult Create()
        {
            CargarUniversidades();
            return View(new Sede());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Sede model)
        {
            if (!ModelState.IsValid)
            {
                CargarUniversidades(model.IdUniversidad);
                return View(model);
            }
            try
            {
                _db.CrearSede(model);
                TempData["Mensaje"] = "Sede creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, OracleErrorHelper.MensajeAmigable(ex));
                CargarUniversidades(model.IdUniversidad);
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var s = _db.ObtenerSede(id);
                if (s == null) return NotFound();
                CargarUniversidades(s.IdUniversidad);
                return View(s);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Sede model)
        {
            if (!ModelState.IsValid)
            {
                CargarUniversidades(model.IdUniversidad);
                return View(model);
            }
            try
            {
                _db.ActualizarSede(model);
                TempData["Mensaje"] = "Sede actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, OracleErrorHelper.MensajeAmigable(ex));
                CargarUniversidades(model.IdUniversidad);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.EliminarSede(id);
                TempData["Mensaje"] = "Sede eliminada correctamente.";
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "No se pudo eliminar: " + OracleErrorHelper.MensajeAmigable(ex);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
