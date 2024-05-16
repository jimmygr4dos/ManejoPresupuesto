﻿using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Enums;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Repositories;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
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

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = 
                await _reportService.ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => new ResultadoObtenerPorSemana()
            {
                Semana = x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso)
                            .Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto)
                          .Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            if (año == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count; i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if(grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = _userService.ObtenerUsuarioId();

            if (año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await _transaccionesRepository.ObtenerPorMes(usuarioId, año);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gasto = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);

                if (transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                } 
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.TransaccionesPorMes = transaccionesAgrupadas;
            modelo.Año = año;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes (int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = _userService.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int año)
        {
            var fechaInicio = new DateTime(año, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = _userService.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = _userService.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });

            var nombreArchivo = $"Manejo Presupuesto - {DateTime.Today.ToString("dd-MM-yyyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);
        }

        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoría"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto"),
            });

            foreach(var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion, 
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId
                );
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File
                    (
                        stream.ToArray(), 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo
                    );
                }
            }
        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = _userService.ObtenerUsuarioId();

            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = start,
                    FechaFin = end
                });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gasto)? "Red" : null
            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var transacciones = await _transaccionesRepository.ObtenerPorUsuarioId(
                new ParametroObtenerTransaccionesPorUsuario()
                {
                    UsuarioId = usuarioId,
                    FechaInicio = fecha,
                    FechaFin = fecha
                });

            return Json(transacciones); 
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
