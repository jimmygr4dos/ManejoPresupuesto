using AutoMapper;
using ManejoPresupuesto.Enums;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Repositories;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IUsersService _userService;
        private readonly ICuentasRepository _cuentasRepository;
        private readonly ICategoriasRepository _categoriasRepository;
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;

        public TransaccionesController(
            IUsersService userService,
            ICuentasRepository cuentasRepository,
            ICategoriasRepository categoriasRepository,
            ITransaccionesRepository transaccionesRepository,
            IReportService reportService,
            IMapper mapper)
        {
            _userService = userService;
            _cuentasRepository = cuentasRepository;
            _categoriasRepository = categoriasRepository;
            _transaccionesRepository = transaccionesRepository;
            _reportService = reportService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = _userService.ObtenerUsuarioId();

            var modelo = await _reportService.ObtenerReporteTransaccinesDetalladas
                                                (usuarioId, mes, año, ViewBag);

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel transaccion)
        {
            var usuarioId = _userService.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                transaccion.Cuentas = await ObtenerCuentas(usuarioId);
                transaccion.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
                return View(transaccion);
            }

            var cuenta = await _cuentasRepository.ObtenerPorId(transaccion.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await _categoriasRepository.ObtenerPorId(transaccion.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            transaccion.UsuarioId = usuarioId;

            if (transaccion.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1; 
            }

            await _transaccionesRepository.Crear(transaccion);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar (int id, string urlRetorno = null)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            
            var existe = await _transaccionesRepository.ObtenerPorId(id, usuarioId);
            if (existe is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = _mapper.Map<TransaccionActualizacionViewModel>(existe);
            transaccion.MontoAnterior = transaccion.Monto;
            if (transaccion.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.MontoAnterior = transaccion.Monto * -1;
            }

            transaccion.CuentaAnteriorId = transaccion.CuentaId;
            transaccion.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            transaccion.Cuentas = await ObtenerCuentas(usuarioId);
            transaccion.UrlRetorno = urlRetorno;

            return View(transaccion);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            
            if (!ModelState.IsValid)
            {
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                return View(modelo);
            }

            var cuenta = await _cuentasRepository.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await _categoriasRepository.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = _mapper.Map<Transaccion>(modelo);

            if(modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1;
            }

            await _transaccionesRepository.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);
            
            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            } else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            
            var transaccion = await _transaccionesRepository.ObtenerPorId(id, usuarioId);
            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _transaccionesRepository.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId) 
        {
            var cuentas = await _cuentasRepository.Obtener(usuarioId);
            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));

        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias
            (int usuarioId, TipoOperacion tipoOperacionId)
        {
            var categorias = await _categoriasRepository.Obtener(usuarioId, tipoOperacionId);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacionId)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacionId);
            return Ok(categorias);
        }

    }
}
