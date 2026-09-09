using BecasPosgrado.Data;
using BecasPosgrado.Filters;
using BecasPosgrado.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace BecasPosgrado.Controllers
{
    [RequiereRol("ADMIN")]
    public class ProgramasController : Controller
    {
        private readonly OracleDbContext _db;

        public static readonly string[] Areas = { "Especialidad", "Maestría", "Doctorado" };

        public ProgramasController(OracleDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            try
            {
                return View(_db.ListarProgramas());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Programa>());
            }
        }

        public IActionResult Create()
        {
            ViewBag.Areas = Areas;
            return View(new Programa());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Programa model)
        {
            ViewBag.Areas = Areas;
            if (!ModelState.IsValid) return View(model);
            try
            {
                _db.CrearPrograma(model);
                TempData["Mensaje"] = "Programa creado correctamente.";
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
                var p = _db.ObtenerPrograma(id);
                if (p == null) return NotFound();
                ViewBag.Areas = Areas;
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
        public IActionResult Edit(Programa model)
        {
            ViewBag.Areas = Areas;
            if (!ModelState.IsValid) return View(model);
            try
            {
                _db.ActualizarPrograma(model);
                TempData["Mensaje"] = "Programa actualizado correctamente.";
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
                _db.EliminarPrograma(id);
                TempData["Mensaje"] = "Programa eliminado correctamente.";
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "No se pudo eliminar: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
