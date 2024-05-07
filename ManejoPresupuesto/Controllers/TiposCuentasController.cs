using ManejoPresupuesto.Models;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController: Controller
    {
        private readonly ITiposCuentasRepository _tiposCuentasRepository;
        private readonly IUsersService _usersService;

        public TiposCuentasController(ITiposCuentasRepository tiposCuentasRepository, IUsersService usersService)
        {
            _tiposCuentasRepository = tiposCuentasRepository;
            _usersService = usersService;
        }

        public async Task<IActionResult> Index ()
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var tiposCuentas = await _tiposCuentasRepository.Obtener(usuarioId);
            return View(tiposCuentas);
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

            tipoCuenta.UsuarioId = _usersService.ObtenerUsuarioId();
            var existe = await _tiposCuentasRepository.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if(existe)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"El nombre {tipoCuenta.Nombre} ya existe!");
                return View(tipoCuenta);
            }

            await _tiposCuentasRepository.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> Editar (int tipoCuentaId)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var tipoCuenta = await _tiposCuentasRepository.ObtenerPorId(tipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar (TipoCuenta tipoCuenta)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var existe = await _tiposCuentasRepository.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if (existe is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _tiposCuentasRepository.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar (int tipoCuentaId)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var tipoCuenta = await _tiposCuentasRepository.ObtenerPorId(tipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(TipoCuenta tipoCuenta)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var existe = await _tiposCuentasRepository.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if (existe is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _tiposCuentasRepository.Borrar(tipoCuenta.Id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var existe = await _tiposCuentasRepository.Existe(nombre, usuarioId);
            
            if (existe)
            {
                return Json($"El nombre {nombre} ya existe!");
            }

            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var tiposCuentas = await _tiposCuentasRepository.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var tiposCuentasOrdenados = ids.Select((valor, indice) =>
                new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await _tiposCuentasRepository.Ordenar(tiposCuentasOrdenados);

            return Ok();
        }

    }
}
