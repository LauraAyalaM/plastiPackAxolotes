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
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);          // ← DateTime.UtcNow.Date → DateOnly
            var userId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var items = await _context.PlanillaItems
                .Include(i => i.Planilla)
                    .ThenInclude(p => p!.Selladora)
                .Include(i => i.OrdenProceso)
                    .ThenInclude(op => op!.OrdenProduccion)
                        .ThenInclude(o => o!.Referencia)
                .Include(i => i.Registros.Where(r => r.OperarioId == userId))

                .Where(i => i.Planilla!.Fecha == hoy && i.Estado != "completado")  // ← .Date eliminado
                .OrderBy(i => i.Planilla!.SelladoraId)                             // ← typo corregido
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

            ViewData["ActivePage"] = "Sellado";
            return View(item);
        }

        [HttpPost]
[Authorize(Roles = "operario,jefe_produccion")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Registrar(
    int planillaItemId,
    string? numeroRollo,
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
        PlanillaItemId   = planillaItemId,
        OperarioId       = userId,
        NumeroRollo      = numeroRollo,
        HoraInicio       = horaInicio,
        HoraFin          = horaFin,
        CantidadUnidades = cantidadUnidades,
        PesoDesperdicio  = pesoDesperdicio,
        Observaciones    = observaciones,
        CreatedAt        = DateTime.UtcNow
    };

    _context.RegistrosSellado.Add(registro);
    await _context.SaveChangesAsync();

    // Verificar si se completó la cantidad requerida
    var item = await _context.PlanillaItems
        .Include(i => i.Registros)
        .Include(i => i.OrdenProceso)
            .ThenInclude(op => op!.OrdenProduccion)
        .FirstOrDefaultAsync(i => i.Id == planillaItemId);

    if (item != null)
    {
        if (item.Estado == "pendiente")
            item.Estado = "en_proceso";

        var totalRegistrado   = item.Registros.Sum(r => r.CantidadUnidades ?? 0);
        var cantidadRequerida = item.OrdenProceso?.OrdenProduccion?.CantidadRequerida ?? 0;

        if (totalRegistrado >= cantidadRequerida)
        {
            item.Estado = "completado";

            if (item.OrdenProceso != null)
            {
                item.OrdenProceso.Estado   = "completado";
                item.OrdenProceso.FechaFin = DateTime.UtcNow;

                var ordenId      = item.OrdenProceso.OrdenProduccionId;
                var todosProcesos = await _context.OrdenProcesos
                    .Where(p => p.OrdenProduccionId == ordenId)
                    .ToListAsync();

                if (todosProcesos.All(p => p.Estado == "completado" || p.Estado == "omitido"))
                {
                    var orden = await _context.OrdenesProduccion.FindAsync(ordenId);
                    if (orden != null) orden.Estado = "completada";
                }
            }
        }

        await _context.SaveChangesAsync();
    }

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
    .Where(r => r.OperarioId == userId)
                .AsQueryable();

            // CreatedAt es DateTime, aquí .Date sí es válido
            if (!string.IsNullOrWhiteSpace(fecha) && DateTime.TryParse(fecha, out var fechaParsed))
                query = query.Where(r => r.CreatedAt.Date == fechaParsed.Date);

            var registros = await query
                .OrderByDescending(r => r.CreatedAt)
                .Take(100)
                .ToListAsync();

            ViewBag.FechaFiltro    = fecha;
            ViewData["ActivePage"] = "MisRegistros";
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