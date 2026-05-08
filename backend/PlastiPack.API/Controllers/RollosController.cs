using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class RollosController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 20;

        public RollosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? estado, string? busqueda, int pagina = 1)
        {
            var query = _context.Rollos
                .Include(r => r.Referencia)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(r => r.Estado == estado);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(r =>
                    r.NumeroRollo.Contains(busqueda) ||
                    r.Referencia!.Codigo.Contains(busqueda) ||
                    (r.MarcaImpresa != null && r.MarcaImpresa.Contains(busqueda)));

            var total = await query.CountAsync();
            var rollos = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Estado       = estado;
            ViewBag.Busqueda     = busqueda;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.Total        = total;
            ViewData["ActivePage"] = "Rollos";
            return View(rollos);
        }

        [Authorize(Roles = "jefe_produccion,operario")]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Referencias = await _context.Referencias
                .Where(r => r.Estado == "Activo")
                .OrderBy(r => r.Codigo)
                .ToListAsync();

            ViewData["ActivePage"] = "Rollos";
            return View(new Rollo());
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion,operario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rollo model)
        {
            var existe = await _context.Rollos
                .AnyAsync(r => r.NumeroRollo == model.NumeroRollo);
            if (existe)
                ModelState.AddModelError("NumeroRollo", "Ya existe un rollo con ese número.");

            if (!ModelState.IsValid)
            {
                ViewBag.Referencias = await _context.Referencias
                    .Where(r => r.Estado == "Activo").OrderBy(r => r.Codigo).ToListAsync();
                ViewData["ActivePage"] = "Rollos";
                return View(model);
            }

            model.Estado    = "disponible";
            model.CreatedAt = DateTime.UtcNow;
            _context.Rollos.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Rollo '{model.NumeroRollo}' registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion,operario")]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var rollo = await _context.Rollos.FindAsync(id);
            if (rollo == null) return NotFound();

            var estadosValidos = new[] { "disponible", "en_proceso", "usado", "defectuoso" };
            if (!estadosValidos.Contains(nuevoEstado))
            {
                TempData["Error"] = "Estado no válido.";
                return RedirectToAction(nameof(Index));
            }

            rollo.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Rollo '{rollo.NumeroRollo}' → {nuevoEstado}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DisponiblesPorReferencia(int referenciaId)
        {
            var rollos = await _context.Rollos
                .Where(r => r.ReferenciaId == referenciaId && r.Estado == "disponible")
                .Select(r => new { r.Id, r.NumeroRollo, r.PesoKg, r.TieneImpresion })
                .ToListAsync();

            return Json(rollos);
        }

        [Authorize(Roles = "jefe_produccion,operario")]
        public async Task<IActionResult> Crear(int? ordenId)
        {
            if (ordenId.HasValue)
            {
                var orden = await _context.OrdenesProduccion
                    .Include(o => o.Referencia)
                    .FirstOrDefaultAsync(o => o.Id == ordenId.Value);

                if (orden != null)
                {
                    ViewBag.OrdenId      = orden.Id;
                    ViewBag.ReferenciaId = orden.ReferenciaId;
                    ViewBag.ReferenciaCodigo = orden.Referencia?.Codigo;
                    ViewBag.ReferenciaNombre = orden.Referencia?.Nombre;
                }
            }

            ViewData["ActivePage"] = "Rollos";
            return View(new Rollo());
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion,operario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rollo model, int? ordenId)
        {
            var existe = await _context.Rollos
                .AnyAsync(r => r.NumeroRollo == model.NumeroRollo);
            if (existe)
                ModelState.AddModelError("NumeroRollo", "Ya existe un rollo con ese número.");

            if (!ModelState.IsValid)
            {
                ViewData["ActivePage"] = "Rollos";
                return View(model);
            }

            model.Estado             = "disponible";
            model.CreatedAt          = DateTime.UtcNow;
            model.OrdenProduccionId  = ordenId;  // ← vincular a la orden

            _context.Rollos.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Rollo '{model.NumeroRollo}' registrado correctamente.";

            // Si vino desde una orden, redirigir de vuelta a ella
            if (ordenId.HasValue)
                return RedirectToAction("Detalle", "OrdenesProduccion", new { id = ordenId.Value });

            return RedirectToAction(nameof(Index));
        }
    }
}
