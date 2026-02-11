using Microsoft.AspNetCore.Mvc;
using MvcCoreProceduresEF.Models;
using MvcCoreProceduresEF.Repositories;

namespace MvcCoreProceduresEF.Controllers
{
    public class DoctoresController : Controller
    {
        private RepositoryDoctores repo;

        public DoctoresController(RepositoryDoctores repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        { 
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            return View(especialidades);
        }
        [HttpPost]
        public async Task<IActionResult> Index(string especialidad, int incremento, string updateopt)
        {
            List<string> especialidades = await this.repo.GetEspecialidadesAsync();
            if (updateopt == "raw")
            {
                await this.repo.UpdateSalarioAsync(especialidad, incremento);
                List<Doctor> doctores = await this.repo.GetDoctoresEspecialidadAsync(especialidad);
                ViewData["DOCTORES"] = doctores;
                ViewData["METODO"] = "procedure";
            }
            else if (updateopt == "ef")
            {
                await this.repo.UpdateSalarioLinqAsync(especialidad, incremento);
                List<Doctor> doctores = await this.repo.GetDoctoresEspecialidadAsync(especialidad);
                ViewData["DOCTORES"] = doctores;
                ViewData["METODO"] = "EF";
            }else if (updateopt == "buscar")
            {
                List<Doctor> doctores = await this.repo.GetDoctoresEspecialidadAsync(especialidad);
                ViewData["DOCTORES"] = doctores;
                ViewData["METODO"] = "Buscar";
            }
            return View(especialidades);
        }
    }
}
