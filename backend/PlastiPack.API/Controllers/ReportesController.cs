using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly AppDbContext _context;

        public ReportesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Reportes";
            return View();
        }

        public async Task<IActionResult> PorTurno(string? fecha, int? selladoaId)
        {
            var fechaFiltro = string.IsNullOrWhiteSpace(fecha)
                ? DateTime.UtcNow.Date
                : DateTime.SpecifyKind(DateTime.Parse(fecha).Date, DateTimeKind.Utc);

            var query = _context.RegistrosSellado
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.Planilla)
                        .ThenInclude(p => p!.Selladora)
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.OrdenProceso)
                        .ThenInclude(op => op!.OrdenProduccion)
                            .ThenInclude(o => o!.Referencia)
                .Include(r => r.Operario)
                .Include(r => r.Rollo)
                .Where(r => r.PlanillaItem!.Planilla!.Fecha.Date == fechaFiltro.Date)
                .AsQueryable();

            if (selladoaId.HasValue)
                query = query.Where(r => r.PlanillaItem!.Planilla!.SelladroaId == selladoaId.Value);

            var registros = await query
                .OrderBy(r => r.PlanillaItem!.Planilla!.SelladroaId)
                .ThenBy(r => r.HoraInicio)
                .ToListAsync();

            var resumenPorSelladora = registros
                .GroupBy(r => r.PlanillaItem!.Planilla!.Selladora?.Nombre ?? "—")
                .Select(g => new
                {
                    Selladora        = g.Key,
                    TotalUnidades    = g.Sum(r => r.CantidadUnidades ?? 0),
                    TotalDesperdicio = g.Sum(r => r.PesoDesperdicio),
                    NumRegistros     = g.Count(),
                    Operarios        = g.Select(r => r.Operario?.Nombre).Distinct().ToList()
                })
                .ToList();

            ViewBag.FechaFiltro         = fechaFiltro.ToString("yyyy-MM-dd");
            ViewBag.FechaDisplay        = fechaFiltro.ToString("dd/MM/yyyy");
            ViewBag.SelladoraId         = selladoaId;
            ViewBag.Selladoras          = await _context.Selladoras.Where(s => s.Activa).OrderBy(s => s.Id).ToListAsync();
            ViewBag.ResumenPorSelladora = resumenPorSelladora;
            ViewBag.TotalUnidades       = registros.Sum(r => r.CantidadUnidades ?? 0);
            ViewBag.TotalDesperdicio    = registros.Sum(r => r.PesoDesperdicio);
            ViewData["ActivePage"]      = "Reportes";
            return View(registros);
        }

        public async Task<IActionResult> PorOperario(string? fecha, Guid? operarioId)
        {
            var fechaFiltro = string.IsNullOrWhiteSpace(fecha)
                ? DateTime.UtcNow.Date
                : DateTime.SpecifyKind(DateTime.Parse(fecha).Date, DateTimeKind.Utc);

            var query = _context.RegistrosSellado
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.Planilla)
                        .ThenInclude(p => p!.Selladora)
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.OrdenProceso)
                        .ThenInclude(op => op!.OrdenProduccion)
                            .ThenInclude(o => o!.Referencia)
                .Include(r => r.Operario)
                .Include(r => r.Rollo)
                .Where(r => r.PlanillaItem!.Planilla!.Fecha.Date == fechaFiltro.Date)
                .AsQueryable();

            if (operarioId.HasValue)
                query = query.Where(r => r.OperarioId == operarioId.Value);

            var registros = await query
                .OrderBy(r => r.Operario!.Nombre)
                .ThenBy(r => r.HoraInicio)
                .ToListAsync();

            var resumenPorOperario = registros
                .GroupBy(r => new { r.OperarioId, Nombre = r.Operario?.Nombre ?? "—" })
                .Select(g => new
                {
                    OperarioId       = g.Key.OperarioId,
                    Nombre           = g.Key.Nombre,
                    TotalUnidades    = g.Sum(r => r.CantidadUnidades ?? 0),
                    TotalDesperdicio = g.Sum(r => r.PesoDesperdicio),
                    NumRegistros     = g.Count(),
                    Rollos           = g.Select(r => r.Rollo?.NumeroRollo).Distinct().ToList()
                })
                .OrderByDescending(x => x.TotalUnidades)
                .ToList();

            var operarios = await _context.Usuarios
                .Where(u => u.Rol != null && u.Rol.Nombre == "operario" && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.FechaFiltro        = fechaFiltro.ToString("yyyy-MM-dd");
            ViewBag.FechaDisplay       = fechaFiltro.ToString("dd/MM/yyyy");
            ViewBag.OperarioIdFiltro   = operarioId;
            ViewBag.Operarios          = operarios;
            ViewBag.ResumenPorOperario = resumenPorOperario;
            ViewBag.TotalUnidades      = registros.Sum(r => r.CantidadUnidades ?? 0);
            ViewBag.TotalDesperdicio   = registros.Sum(r => r.PesoDesperdicio);
            ViewData["ActivePage"]     = "Reportes";
            return View(registros);
        }

        public async Task<IActionResult> ProduccionGeneral(string? desde, string? hasta)
        {
            var fechaDesde = string.IsNullOrWhiteSpace(desde)
                ? DateTime.UtcNow.Date.AddDays(-30)
                : DateTime.SpecifyKind(DateTime.Parse(desde).Date, DateTimeKind.Utc);
            var fechaHasta = string.IsNullOrWhiteSpace(hasta)
                ? DateTime.UtcNow.Date
                : DateTime.SpecifyKind(DateTime.Parse(hasta).Date, DateTimeKind.Utc);

            var registros = await _context.RegistrosSellado
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.Planilla)
                .Include(r => r.PlanillaItem)
                    .ThenInclude(i => i!.OrdenProceso)
                        .ThenInclude(op => op!.OrdenProduccion)
                            .ThenInclude(o => o!.Referencia)
                .Include(r => r.Operario)
                .Where(r => r.PlanillaItem!.Planilla!.Fecha.Date >= fechaDesde.Date
                         && r.PlanillaItem!.Planilla!.Fecha.Date <= fechaHasta.Date)
                .ToListAsync();

            var porDia = registros
                .GroupBy(r => r.PlanillaItem!.Planilla!.Fecha.Date)
                .Select(g => new
                {
                    Fecha            = g.Key,
                    TotalUnidades    = g.Sum(r => r.CantidadUnidades ?? 0),
                    TotalDesperdicio = g.Sum(r => r.PesoDesperdicio),
                    NumRegistros     = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            var porReferencia = registros
                .GroupBy(r => r.PlanillaItem?.OrdenProceso?.OrdenProduccion?.Referencia?.Codigo ?? "—")
                .Select(g => new
                {
                    Codigo        = g.Key,
                    TotalUnidades = g.Sum(r => r.CantidadUnidades ?? 0),
                    NumRegistros  = g.Count()
                })
                .OrderByDescending(x => x.TotalUnidades)
                .Take(10)
                .ToList();

            ViewBag.FechaDesde       = fechaDesde.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta       = fechaHasta.ToString("yyyy-MM-dd");
            ViewBag.PorDia           = porDia;
            ViewBag.PorReferencia    = porReferencia;
            ViewBag.TotalUnidades    = registros.Sum(r => r.CantidadUnidades ?? 0);
            ViewBag.TotalDesperdicio = registros.Sum(r => r.PesoDesperdicio);
            ViewBag.DiasConRegistro  = porDia.Count;
            ViewData["ActivePage"]   = "Reportes";
            return View();
        }
    }
}