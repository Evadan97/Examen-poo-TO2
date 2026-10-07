using Newtonsoft.Json;
using TO2.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace TO2.Controllers
{
    public class AlumnoController : Controller
    {
        static string jlistAlumnos = "[]";

        public ActionResult Index()
        {
            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);

            // Aqui Deserializar el string JSON -> List<Alumno> para la vista.
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();
            return View(temporal);
        }

        public ActionResult Agregar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(Alumno alumno)
        {
            if (alumno.dni != null) alumno.dni = alumno.dni.Trim();
            if (string.IsNullOrWhiteSpace(alumno.dni))
                ModelState.AddModelError("dni", "Ingrese el DNI");
            if (string.IsNullOrWhiteSpace(alumno.nombres))
                ModelState.AddModelError("nombres", "Ingrese sus nombres");
            if (string.IsNullOrWhiteSpace(alumno.apellidos))
                ModelState.AddModelError("apellidos", "Ingrese sus apellidos");
            if (string.IsNullOrWhiteSpace(alumno.carrera))
                ModelState.AddModelError("carrera", "Ingrese la carrera");
            if (alumno.ciclo < 1 || alumno.ciclo > 10)
                ModelState.AddModelError("ciclo", "El ciclo debe estar entre 1 y 10");
            if (!ModelState.IsValid) return View(alumno);

            //Deserializar la lista actual desde el archivo JSON.
            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();

            // VERIFICACION el DNI no debe existir antes de agregar al alumno.
            if (temporal.Exists(a => string.Equals(a.dni, alumno.dni, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("dni", "Ya existe este DNI");
                return View(alumno);
            }

            temporal.Add(alumno);

            // Guardar la colección y volver a Index mostrando la confirmación.
            return Serializar(temporal);
        }

        public ActionResult Detalles(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Aviso"] = "Selecciona un alumno de la lista para ver sus detalles.";
                return RedirectToAction("Index");
            }

            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();

            // Buscar al alumno por DNI y mostrar todos sus datos.
            Alumno alumnoEncontrado = temporal.Find(a => string.Equals(a.dni, id, StringComparison.OrdinalIgnoreCase));
            if (alumnoEncontrado == null) return HttpNotFound();
            return View(alumnoEncontrado);
        }

        public ActionResult Actualizar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Aviso"] = "Selecciona un alumno de la lista para actualizarlo.";
                return RedirectToAction("Index");
            }

            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();

            Alumno alumnoEncontrado = temporal.Find(a => string.Equals(a.dni, id, StringComparison.OrdinalIgnoreCase));
            if (alumnoEncontrado == null) return HttpNotFound();
            ViewBag.DniOriginal = alumnoEncontrado.dni;
            return View(alumnoEncontrado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Actualizar(string dniOriginal, Alumno alumno)
        {
            if (string.IsNullOrWhiteSpace(dniOriginal)) return HttpNotFound();
            ViewBag.DniOriginal = dniOriginal;
            if (alumno.dni != null) alumno.dni = alumno.dni.Trim();
            if (string.IsNullOrWhiteSpace(alumno.dni))
                ModelState.AddModelError("dni", "Ingrese el DNI");
            if (string.IsNullOrWhiteSpace(alumno.nombres))
                ModelState.AddModelError("nombres", "Ingrese los nombres");
            if (string.IsNullOrWhiteSpace(alumno.apellidos))
                ModelState.AddModelError("apellidos", "Ingrese los apellidos");
            if (string.IsNullOrWhiteSpace(alumno.carrera))
                ModelState.AddModelError("carrera", "Ingrese la carrera");
            if (alumno.ciclo < 1 || alumno.ciclo > 10)
                ModelState.AddModelError("ciclo", "El ciclo debe estar entre 1 y 10");
            if (!ModelState.IsValid) return View(alumno);

            // Deserializar y buscar la posición del alumno por su DNI original.
            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();
            int index = temporal.FindIndex(a => string.Equals(a.dni, dniOriginal, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return HttpNotFound();

            // Permitir conservar su DNI, pero no usar el DNI de otro alumno.
            if (temporal.Exists(a => !string.Equals(a.dni, dniOriginal, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.dni, alumno.dni, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("dni", "Ya existe otro alumno con ese DNI");
                return View(alumno);
            }

            temporal[index] = alumno;

            // Guardar la lista actualizada en el archivo JSON.
            return Serializar(temporal);
        }

        public ActionResult Eliminar(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Aviso"] = "Selecciona un alumno de la lista para eliminarlo.";
                return RedirectToAction("Index");
            }

            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();

            Alumno alumnoEncontrado = temporal.Find(a => string.Equals(a.dni, id, StringComparison.OrdinalIgnoreCase));
            if (alumnoEncontrado == null) return HttpNotFound();
            return View(alumnoEncontrado);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfirmado(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return HttpNotFound();

            // Deserializar la lista y buscar el alumno por DNI.
            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            if (System.IO.File.Exists(ruta))
                jlistAlumnos = System.IO.File.ReadAllText(ruta);
            List<Alumno> temporal = JsonConvert.DeserializeObject<List<Alumno>>(jlistAlumnos) ?? new List<Alumno>();
            Alumno alumnoEncontrado = temporal.Find(a => string.Equals(a.dni, id, StringComparison.OrdinalIgnoreCase));
            if (alumnoEncontrado == null) return HttpNotFound();

            // Quitar el registro, serializar y regresar a Index.
            temporal.Remove(alumnoEncontrado);
            return Serializar(temporal);
        }

        [NonAction]
        public ActionResult Serializar(List<Alumno> temporal)
        {
            // Convertir la colección en JSON y guardarla sin necesidad de un botón.
            string ruta = Server.MapPath("~/App_Data/Alumnos.json");
            jlistAlumnos = JsonConvert.SerializeObject(temporal, Formatting.Indented);
            System.IO.File.WriteAllText(ruta, jlistAlumnos);

            // Index muestra este mensaje y lo oculta después de cinco segundos.
            TempData["Mensaje"] = "Colección serializada en App_Data/Alumnos.json";
            return RedirectToAction("Index");
        }
    }
}
