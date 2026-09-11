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
    public class ComponentesController : Controller
    {
        private readonly OracleDbContext _db;

        public ComponentesController(OracleDbContext db)
        {
            _db = db;
        }

        private void CargarProgramas(int? seleccionado = null)
        {
            ViewBag.Programas = new SelectList(_db.ListarProgramas(), "Id", "Nombre", seleccionado);
        }

        public IActionResult Index()
        {
            try
            {
                return View(_db.ListarComponentes());
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return View(new List<Componente>());
            }
        }

        public IActionResult Create()
        {
            CargarProgramas();
            return View(new Componente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Componente model)
        {
            if (!ModelState.IsValid)
            {
                CargarProgramas(model.IdPrograma);
                return View(model);
            }
            try
            {
                _db.CrearComponente(model);
                TempData["Mensaje"] = "Componente creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, OracleErrorHelper.MensajeAmigable(ex));
                CargarProgramas(model.IdPrograma);
                return View(model);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var c = _db.ObtenerComponente(id);
                if (c == null) return NotFound();
                CargarProgramas(c.IdPrograma);
                return View(c);
            }
            catch (OracleException ex)
            {
                ViewBag.Error = OracleErrorHelper.MensajeAmigable(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Componente model)
        {
            if (!ModelState.IsValid)
            {
                CargarProgramas(model.IdPrograma);
                return View(model);
            }
            try
            {
                _db.ActualizarComponente(model);
                TempData["Mensaje"] = "Componente actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (OracleException ex)
            {
                ModelState.AddModelError(string.Empty, OracleErrorHelper.MensajeAmigable(ex));
                CargarProgramas(model.IdPrograma);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.EliminarComponente(id);
                TempData["Mensaje"] = "Componente eliminado correctamente.";
            }
            catch (OracleException ex)
            {
                TempData["Mensaje"] = "No se pudo eliminar: " + OracleErrorHelper.MensajeAmigable(ex);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
