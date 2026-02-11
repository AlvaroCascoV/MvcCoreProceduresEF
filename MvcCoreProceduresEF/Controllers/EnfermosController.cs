using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class EnfermosController : Controller
    {
        private RepositoryEnfermos repo;

        public EnfermosController(RepositoryEnfermos repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<Enfermo> enfermos = await this.repo.GetEnfermosAsync();
            return View(enfermos);
        }
        public async Task<IActionResult> Details(string inscripcion)
        {
            Enfermo enfermo = await this.repo.FindEnfermoAsync(inscripcion);
            return View(enfermo);
        }
        public async Task<IActionResult> Delete(string inscripcion)
        {
            await this.repo.DeleteEnfermoAsync(inscripcion);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteRaw(string inscripcion)
        {
            await this.repo.DeleteEnfermoRawAsync(inscripcion);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Insert() 
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Insert(Enfermo enf)
        {
            await this.repo.InserEnfermoAsync(enf.Apellido, enf.Direccion, enf.FechaNacimiento, enf.Genero, enf.Nss);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> InsertRaw() 
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> InsertRaw(Enfermo enf)
        {
            await this.repo.InserEnfermoRawAsync(enf.Apellido, enf.Direccion, enf.FechaNacimiento, enf.Genero, enf.Nss);
            return RedirectToAction("Index");
        }
    }
}
