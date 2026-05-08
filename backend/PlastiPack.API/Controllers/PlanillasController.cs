using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class PlanillasController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 20;

        public PlanillasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? fecha, int? selladoraId, int pagina = 1)
        {
            var query = _context.Planillas
                .Include(p => p.Selladora)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(fecha) &&
                DateOnly.TryParse(fecha, out var fechaParsed))
            {
                query = query.Where(p => p.Fecha == fechaParsed);
            }

            if (selladoraId.HasValue)
                query = query.Where(p => p.SelladoraId == selladoraId.Value);

            var total     = await query.CountAsync();
            var planillas = await query
                .OrderByDescending(p => p.Fecha)
                .ThenBy(p => p.SelladoraId)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Selladoras     = await _context.Selladoras.Where(s => s.Activa).OrderBy(s => s.Id).ToListAsync();
            ViewBag.FechaFiltro    = fecha;
            ViewBag.SelladoraId    = selladoraId;
            ViewBag.PaginaActual   = pagina;
            ViewBag.TotalPaginas   = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.Total          = total;
            ViewData["ActivePage"] = "Planillas";
            return View(planillas);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var planilla = await _context.Planillas
                .Include(p => p.Selladora)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.Items.OrderBy(i => i.Posicion))
                    .ThenInclude(i => i.OrdenProceso)
                        .ThenInclude(op => op!.OrdenProduccion)
                            .ThenInclude(o => o!.Referencia)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Registros)
                        .ThenInclude(r => r.Operario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (planilla == null) return NotFound();

            ViewData["ActivePage"] = "Planillas";
            return View(planilla);
        }

        [Authorize(Roles = "jefe_produccion")]
        public async Task<IActionResult> Crear()
        {
            var selladoras = await _context.Selladoras
                .Where(s => s.Activa)
                .OrderBy(s => s.Id)
                .ToListAsync();

            // IDs de procesos que ya están asignados a alguna planilla
            var idsYaAsignados = await _context.PlanillaItems
                .Select(pi => pi.OrdenProcesoId)
                .ToHashSetAsync();

            // Solo procesos de sellado:
            //   - cuya orden esté en_sellado
            //   - que estén pendientes (no iniciados ni completados)
            //   - que no estén asignados a ninguna planilla existente
            var procesosSellado = await _context.OrdenProcesos
                .Include(op => op.OrdenProduccion)
                    .ThenInclude(o => o!.Referencia)
                .Include(op => op.OrdenProduccion)
                    .ThenInclude(o => o!.Pedido)
                        .ThenInclude(p => p!.Cliente)
                .Where(op => op.NombreProceso == "sellado"
          && (op.Estado == "pendiente" || op.Estado == "en_proceso") // ← agregar en_proceso
          && op.OrdenProduccion!.Estado == "en_sellado"
          && !idsYaAsignados.Contains(op.Id))
                .OrderBy(op => op.OrdenProduccion!.Pedido!.FechaEntrega)
                .ToListAsync();

            ViewBag.Selladoras      = selladoras;
            ViewBag.ProcesosSellado = procesosSellado;
            ViewBag.FechaHoy        = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
            ViewData["ActivePage"]  = "Planillas";
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int selladoraId, string fecha, List<int> procesosIds)
        {
            if (!procesosIds.Any())
            {
                TempData["Error"] = "Debes agregar al menos un proceso a la planilla.";
                return RedirectToAction(nameof(Crear));
            }

            if (!DateOnly.TryParse(fecha, out var fechaDate))
            {
                TempData["Error"] = "Fecha inválida.";
                return RedirectToAction(nameof(Crear));
            }

            // IDs de procesos que ya están asignados a alguna planilla
            var idsYaAsignados = await _context.PlanillaItems
                .Select(pi => pi.OrdenProcesoId)
                .ToHashSetAsync();

            // Validar que todos los procesos enviados sean sellado de órdenes en_sellado,
            // que estén pendientes y que no tengan planilla asignada
            var procesosValidos = await _context.OrdenProcesos
                .Include(op => op.OrdenProduccion)
                .Where(op => procesosIds.Contains(op.Id)
                          && op.NombreProceso == "sellado"
                          && (op.Estado == "pendiente" || op.Estado == "en_proceso")
                          && op.OrdenProduccion!.Estado == "en_sellado"
                          && !idsYaAsignados.Contains(op.Id))
                .ToListAsync();

            if (procesosValidos.Count != procesosIds.Count)
            {
                TempData["Error"] = "Algunos procesos seleccionados no están disponibles para sellado " +
                                    "(ya fueron asignados, iniciados o completados).";
                return RedirectToAction(nameof(Crear));
            }

            var existe = await _context.Planillas
                .AnyAsync(p => p.SelladoraId == selladoraId && p.Fecha == fechaDate);

            if (existe)
            {
                TempData["Error"] = "Ya existe una planilla para esa selladora en esa fecha.";
                return RedirectToAction(nameof(Crear));
            }

            var userId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var planilla = new Planilla
            {
                SelladoraId = selladoraId,
                Fecha       = fechaDate,
                CreadoPor   = userId,
                CreatedAt   = DateTime.UtcNow
            };

            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync();

            var items = procesosValidos.Select((p, idx) => new PlanillaItem
            {
                PlanillaId     = planilla.Id,
                OrdenProcesoId = p.Id,
                Posicion       = idx + 1,
                Estado         = "pendiente"
            }).ToList();

            _context.PlanillaItems.AddRange(items);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Planilla creada con {items.Count} ítem(s) para {fechaDate:dd/MM/yyyy}.";
            return RedirectToAction(nameof(Detalle), new { id = planilla.Id });
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        public async Task<IActionResult> ActualizarItem(int itemId, string nuevoEstado)
        {
            var item = await _context.PlanillaItems
                .Include(i => i.Planilla)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return NotFound();

            item.Estado = nuevoEstado;

            if (nuevoEstado == "en_proceso")
            {
                var proceso = await _context.OrdenProcesos.FindAsync(item.OrdenProcesoId);
                if (proceso != null && proceso.Estado == "pendiente")
                {
                    proceso.Estado      = "en_proceso";
                    proceso.FechaInicio = DateTime.UtcNow;
                    // La orden ya está en en_sellado, no se modifica
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Estado del ítem actualizado.";
            return RedirectToAction(nameof(Detalle), new { id = item.PlanillaId });
        }
    }
}