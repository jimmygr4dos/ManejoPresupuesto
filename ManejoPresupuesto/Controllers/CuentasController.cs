using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Repositories;
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
        private readonly ITransaccionesRepository _transaccionesRepository;
        private readonly IMapper _mapper;

        public CuentasController(
            ITiposCuentasRepository tiposCuentasRepository, 
            IUsersService usersService,
            ICuentasRepository cuentasRepository,
            ITransaccionesRepository transaccionesRepository,
            IMapper mapper)
        {
            _tiposCuentasRepository = tiposCuentasRepository;
            _usersService = usersService;
            _cuentasRepository = cuentasRepository;
            _transaccionesRepository = transaccionesRepository;
            _mapper = mapper;
        }

        public async Task<IActionResult> Detalle(int id, int mes, int año)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var cuenta = await _cuentasRepository.ObtenerPorId(id, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            DateTime fechaInicio;
            DateTime fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }

            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var obtenerTransaccionesPorCuenta = new ObtenerTransaccionesPorCuenta()
            {
                CuentaId = id,
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var transacciones = await _transaccionesRepository
                                        .ObtenerPorCuentaId(obtenerTransaccionesPorCuenta);

            var modelo = new ReporteTransaccionesDetalladas();
            ViewBag.Nombre = cuenta.Nombre;

            var transaccionesPorFecha = transacciones
                                        .OrderByDescending(x => x.FechaTransaccion)
                                        .GroupBy(x => x.FechaTransaccion)
                                        .Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFecha()
                                        {
                                            FechaTransaccion = grupo.Key,
                                            Transacciones = grupo.AsEnumerable()
                                        });

            modelo.TransaccionesAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1).Month;
            ViewBag.añoAnterior = fechaInicio.AddMonths(-1).Year;
            ViewBag.mesPosterior = fechaInicio.AddMonths(1).Month;
            ViewBag.añoPosterior = fechaInicio.AddMonths(1).Year;
            ViewBag.urlRetorno = HttpContext.Request.Path + HttpContext.Request.QueryString;

            return View(modelo);
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

        [HttpGet]
        public async Task<IActionResult> Editar (int cuentaId)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var cuenta = await _cuentasRepository.ObtenerPorId(cuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            //var modelo = new CuentaCreacionViewModel
            //{
            //    Id = cuenta.Id,
            //    Nombre = cuenta.Nombre,
            //    TipoCuentaId = cuenta.TipoCuentaId,
            //    Descripcion = cuenta.Descripcion,
            //    Balance = cuenta.Balance
            //};

            var modelo = _mapper.Map<CuentaCreacionViewModel>(cuenta);

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
            
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCreacionViewModel cuenta)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var cuentaExiste = _cuentasRepository.ObtenerPorId(cuenta.Id, usuarioId);

            if (cuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = await _tiposCuentasRepository.ObtenerPorId(cuenta.TipoCuentaId, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _cuentasRepository.Actualizar(cuenta);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar (int cuentaId)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var cuenta = await _cuentasRepository.ObtenerPorId(cuentaId, usuarioId);

            if (cuenta is null) {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta (Cuenta cuenta)
        {
            var usuarioId = _usersService.ObtenerUsuarioId();
            var existe = await _cuentasRepository.ObtenerPorId(cuenta.Id, usuarioId);

            if (existe is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _cuentasRepository.Borrar(cuenta.Id);
            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas (int usuarioId)
        {
            var tiposCuentas = await _tiposCuentasRepository.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}
