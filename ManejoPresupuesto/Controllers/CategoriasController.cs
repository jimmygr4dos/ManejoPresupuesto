using ManejoPresupuesto.Models;
using ManejoPresupuesto.Repositories;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ICategoriasRepository _categoriasRepository;
        private readonly IUsersService _userService;

        public CategoriasController(
            ICategoriasRepository categoriasRepository,
            IUsersService userService)
        {
            _categoriasRepository = categoriasRepository;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacionViewModel)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var categorias = await _categoriasRepository.Obtener(usuarioId, paginacionViewModel);
            var totalCategorias = await _categoriasRepository.Contar(usuarioId);

            var respuestaVM = new PaginacionRespuesta<Categoria>
            {
                Elementos = categorias,
                Pagina = paginacionViewModel.Pagina,
                RecordsPorPagina = paginacionViewModel.RecordsPorPagina,
                CantidadTotalRecords = totalCategorias,
                //BaseURL = "/categorias"
                BaseURL = Url.Action()
            };

            return View(respuestaVM);
            //return View(categorias);
        }

        [HttpGet]
        public IActionResult Crear() 
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = _userService.ObtenerUsuarioId();

            categoria.UsuarioId = usuarioId;

            await _categoriasRepository.Crear(categoria);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int categoriaId)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var categoria = await _categoriasRepository.ObtenerPorId(categoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = _userService.ObtenerUsuarioId();
            var existe = _categoriasRepository.ObtenerPorId(categoria.Id, usuarioId);

            if (existe is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoria.UsuarioId = usuarioId;

            await _categoriasRepository.Actualizar(categoria);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar (int categoriaId)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var categoria = await _categoriasRepository.ObtenerPorId(categoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(Categoria categoria)
        {
            var usuarioId = _userService.ObtenerUsuarioId();
            var existe = await _categoriasRepository.ObtenerPorId(categoria.Id, usuarioId);

            if (existe is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await _categoriasRepository.Borrar(categoria.Id);
            return RedirectToAction("Index");
        }
        
    }
}
