using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class ReferenciasController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 20;

        public ReferenciasController(AppDbContext context)
        {
            _context = context;
        }

        // ── LISTADO CON FILTROS Y PAGINACIÓN ──
        public async Task<IActionResult> Index(
            string? busqueda,
            string? estado,
            string? tipoProducto,
            string? materiaPrima,
            string? grupo,
            int pagina = 1)
        {
            var query = _context.Referencias
                .Include(r => r.Inventario)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(r =>
                    r.Codigo.Contains(busqueda) ||
                    r.Nombre!.Contains(busqueda) ||
                    r.ReferenciCorta!.Contains(busqueda));

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(r => r.Estado == estado);

            if (!string.IsNullOrWhiteSpace(tipoProducto))
                query = query.Where(r => r.TipoProducto == tipoProducto);

            if (!string.IsNullOrWhiteSpace(materiaPrima))
                query = query.Where(r => r.MateriaPrima == materiaPrima);

            if (!string.IsNullOrWhiteSpace(grupo))
                query = query.Where(r => r.Grupo == grupo);

            var total = await query.CountAsync();
            var referencias = await query
                .OrderBy(r => r.Codigo)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Busqueda    = busqueda;
            ViewBag.Estado      = estado;
            ViewBag.TipoProducto = tipoProducto;
            ViewBag.MateriaPrima = materiaPrima;
            ViewBag.Grupo       = grupo;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.Total       = total;

            ViewBag.Grupos = await _context.Referencias
                .Where(r => r.Grupo != null)
                .Select(r => r.Grupo!)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();

            ViewData["ActivePage"] = "Referencias";
            return View(referencias);
        }

        // ── DETALLE ──
        public async Task<IActionResult> Detalle(int id)
        {
            var referencia = await _context.Referencias
                .Include(r => r.Inventario)
                .Include(r => r.Precios)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (referencia == null) return NotFound();

            ViewData["ActivePage"] = "Referencias";
            return View(referencia);
        }

        // ── CREAR GET ──
        [Authorize(Roles = "jefe_produccion")]
        public IActionResult Crear()
        {
            ViewData["ActivePage"] = "Referencias";
            return View(new Referencia());
        }

        // ── CREAR POST ──
        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Referencia model)
        {
            ModelState.Remove("UsuarioCreador");
            ModelState.Remove("Inventario");
            ModelState.Remove("Precios");

            if (!ModelState.IsValid)
            {
                ViewData["ActivePage"] = "Referencias";
                return View(model);
            }

            var userIdStr = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdStr, out var userId))
                model.CreadoPor = userId;

            model.CreatedAt = DateTime.UtcNow;

            _context.Referencias.Add(model);
            await _context.SaveChangesAsync();

            // Crear registro de inventario vacío
            _context.Inventario.Add(new Inventario
            {
                ReferenciaId = model.Id,
                StockDisponible = 0,
                StockReservado = 0
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Referencia '{model.Codigo}' creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDITAR GET ──
        [Authorize(Roles = "jefe_produccion")]
        public async Task<IActionResult> Editar(int id)
        {
            var referencia = await _context.Referencias
                .Include(r => r.Inventario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (referencia == null) return NotFound();

            ViewData["ActivePage"] = "Referencias";
            return View(referencia);
        }

        // ── EDITAR POST ──
        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Referencia model)
        {
            if (id != model.Id) return BadRequest();

            ModelState.Remove("UsuarioCreador");
            ModelState.Remove("Inventario");
            ModelState.Remove("Precios");

            if (!ModelState.IsValid)
            {
                ViewData["ActivePage"] = "Referencias";
                return View(model);
            }

            var existente = await _context.Referencias.FindAsync(id);
            if (existente == null) return NotFound();

            // Actualizar campos
            existente.Codigo          = model.Codigo;
            existente.ReferenciCorta  = model.ReferenciCorta;
            existente.Nombre          = model.Nombre;
            existente.Grupo           = model.Grupo;
            existente.Estado          = model.Estado;
            existente.TipoProducto    = model.TipoProducto;
            existente.MateriaPrima    = model.MateriaPrima;
            existente.Color           = model.Color;
            existente.Troquelado      = model.Troquelado;
            existente.Ancho           = model.Ancho;
            existente.FuelleIzquierdo = model.FuelleIzquierdo;
            existente.FuelleDerecho   = model.FuelleDerecho;
            existente.Alto            = model.Alto;
            existente.FuelleSuperior  = model.FuelleSuperior;
            existente.FuelleFondo     = model.FuelleFondo;
            existente.Calibre         = model.Calibre;
            existente.Impresion       = model.Impresion;
            existente.ColoresImpresion = model.ColoresImpresion;
            existente.TipoCliente     = model.TipoCliente;
            existente.TipoImpresion   = model.TipoImpresion;
            existente.TipoSellado     = model.TipoSellado;
            existente.TratadoCara     = model.TratadoCara;
            existente.Medida          = model.Medida;
            existente.CostoProduccion = model.CostoProduccion;
            existente.Impuesto        = model.Impuesto;
            existente.CodigoBarras    = model.CodigoBarras;
            existente.Presentacion    = model.Presentacion;
            existente.UnidadMedida    = model.UnidadMedida;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Referencia '{existente.Codigo}' actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ── CAMBIAR ESTADO (Activo/Inactivo) ──
        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var referencia = await _context.Referencias.FindAsync(id);
            if (referencia == null) return NotFound();

            referencia.Estado = referencia.Estado == "Activo" ? "Inactivo" : "Activo";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado cambiado a '{referencia.Estado}'.";
            return RedirectToAction(nameof(Index));
        }

        // ── BUSQUEDA AJAX (para otros módulos) ──
        [HttpGet]
        public async Task<IActionResult> Buscar(string q)
        {
            var resultados = await _context.Referencias
                .Include(r => r.Inventario)
                .Where(r => r.Estado == "Activo" &&
                           (r.Codigo.Contains(q) || r.Nombre!.Contains(q)))
                .Take(10)
                .Select(r => new {
                    r.Id,
                    r.Codigo,
                    r.Nombre,
                    Stock = r.Inventario != null ? r.Inventario.StockDisponible : 0
                })
                .ToListAsync();

            return Json(resultados);
        }
    }
}
