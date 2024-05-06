using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly ITiposCuentasRepository _tiposCuentasRepository;

        public TiposCuentasController(ITiposCuentasRepository tiposCuentasRepository)
        {
            _tiposCuentasRepository = tiposCuentasRepository;
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = 1;
            var existe = await _tiposCuentasRepository.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if(existe)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe!");
                return View(tipoCuenta);
            }

            await _tiposCuentasRepository.Crear(tipoCuenta);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = 1;
            var existe = await _tiposCuentasRepository.Existe(nombre, usuarioId);
            
            if (existe)
            {
                return Json($"El nombre {nombre} ya existe!");
            }

            return Json(true);
        }
    }
}
