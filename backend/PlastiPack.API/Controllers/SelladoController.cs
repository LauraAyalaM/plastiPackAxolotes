using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class SelladoController : Controller
    {
        private readonly AppDbContext _context;

        public SelladoController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.UtcNow.Date;
            var userId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var items = await _context.PlanillaItems
                .Include(i => i.Planilla)
                    .ThenInclude(p => p!.Selladora)
                .Include(i => i.OrdenProceso)
                    .ThenInclude(op => op!.OrdenProduccion)
                        .ThenInclude(o => o!.Referencia)
                .Include(i => i.Registros.Where(r => r.OperarioId == userId))
                    .ThenInclude(r => r.Rollo)
                .Where(i => i.Planilla!.Fecha.Date == hoy && i.Estado != "completado")
                .OrderBy(i => i.Planilla!.SelladroaId)
                .ThenBy(i => i.Posicion)
                .ToListAsync();

            ViewData["ActivePage"] = "Sellado";
            return View(items);
        }

        [Authorize(Roles = "operario,jefe_produccion")]
        public async Task<IActionResult> Registrar(int id)
        {
            var item = await _context.PlanillaItems
                .Include(i => i.Planilla)
                    .ThenInclude(p => p!.Selladora)
                .Include(i => i.OrdenProceso)
                    .ThenInclude(op => op!.OrdenProduccion)
                        .ThenInclude(o => o!.Referencia)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null) return NotFound();

            var referenciaId = item.OrdenProceso?.OrdenProduccion?.ReferenciaId;
            var rollos = await _context.Rollos
                .Where(r => r.ReferenciaId == referenciaId
                         && (r.Estado == "disponible" || r.Estado == "en_proceso"))
                .OrderBy(r => r.NumeroRollo)
                .ToListAsync();

            ViewBag.Rollos = rollos;
            ViewData["ActivePage"] = "Sellado";
            return View(item);
        }

        [HttpPost]
        [Authorize(Roles = "operario,jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(
            int planillaItemId,
            int rolloId,
            TimeOnly horaInicio,
            TimeOnly? horaFin,
            int? cantidadUnidades,
            decimal pesoDesperdicio,
            string? observaciones)
        {
            if (horaFin.HasValue && horaFin <= horaInicio)
            {
                TempData["Error"] = "La hora de fin debe ser posterior a la hora de inicio.";
                return RedirectToAction(nameof(Registrar), new { id = planillaItemId });
            }

            var userId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var registro = new RegistroSellado
            {
                PlanillaItemId    = planillaItemId,
                OperarioId        = userId,
                RolloId           = rolloId,
                HoraInicio        = horaInicio,
                HoraFin           = horaFin,
                CantidadUnidades  = cantidadUnidades,
                PesoDesperdicio   = pesoDesperdicio,
                Observaciones     = observaciones,
                CreatedAt         = DateTime.UtcNow
            };

            _context.RegistrosSellado.Add(registro);

            var rollo = await _context.Rollos.FindAsync(rolloId);
            if (rollo != null && rollo.Estado == "disponible")
                rollo.Estado = "en_proceso";

            var item = await _context.PlanillaItems.FindAsync(planillaItemId);
            if (item != null && item.Estado == "pendiente")
                item.Estado = "en_proceso";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Registro de sellado guardado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MisRegistros(string? fecha)
        {
            var userId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var query = _context.RegistrosSellado
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.Planilla)
                        .ThenInclude(p => p!.Selladora)
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.OrdenProceso)
                        .ThenInclude(op => op!.OrdenProduccion)
                            .ThenInclude(o => o!.Referencia)
                .Include(r => r.Rollo)
                .Where(r => r.OperarioId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(fecha) && DateTime.TryParse(fecha, out var fechaParsed))
                query = query.Where(r => r.CreatedAt.Date == fechaParsed.Date);

            var registros = await query
                .OrderByDescending(r => r.CreatedAt)
                .Take(100)
                .ToListAsync();

            ViewBag.FechaFiltro    = fecha;
            ViewData["ActivePage"] = "Sellado";
            return View(registros);
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        public async Task<IActionResult> CompletarItem(int itemId)
        {
            var item = await _context.PlanillaItems
                .Include(i => i.OrdenProceso)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return NotFound();

            item.Estado = "completado";

            if (item.OrdenProceso != null)
            {
                item.OrdenProceso.Estado   = "completado";
                item.OrdenProceso.FechaFin = DateTime.UtcNow;

                var ordenId = item.OrdenProceso.OrdenProduccionId;
                var todosProcesos = await _context.OrdenProcesos
                    .Where(p => p.OrdenProduccionId == ordenId)
                    .ToListAsync();

                if (todosProcesos.All(p => p.Estado == "completado" || p.Estado == "omitido"))
                {
                    var orden = await _context.OrdenesProduccion.FindAsync(ordenId);
                    if (orden != null) orden.Estado = "completada";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Ítem marcado como completado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
