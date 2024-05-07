using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController: Controller
    {
        private readonly ITiposCuentasRepository _tiposCuentasRepository;
        private readonly IUsersService _usersService;
        private readonly ICuentasRepository _cuentasRepository;

        public CuentasController(
            ITiposCuentasRepository tiposCuentasRepository, 
            IUsersService usersService,
            ICuentasRepository cuentasRepository)
        {
            _tiposCuentasRepository = tiposCuentasRepository;
            _usersService = usersService;
            _cuentasRepository = cuentasRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var modelo = new CuentaCreacionViewModel();
            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            return View(modelo);
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await _cuentasRepository.Obtener(usuarioId);

            var modelo = cuentasConTipoCuenta
                .GroupBy(x => x.TipoCuenta)
                .Select(grupo => new IndiceCuentasViewModel
                {
                    TipoCuenta = grupo.Key,
                    Cuentas = grupo.AsEnumerable()
                }).ToList();

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var tipoCuenta = await _tiposCuentasRepository.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await _cuentasRepository.Crear(cuenta);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas (int usuarioId)
        {
            var tiposCuentas = await _tiposCuentasRepository.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
