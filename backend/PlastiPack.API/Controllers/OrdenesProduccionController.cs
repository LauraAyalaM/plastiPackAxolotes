using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Data;
using PlastiPack.API.Models;

namespace PlastiPack.API.Controllers
{
    [Authorize]
    public class OrdenesProduccionController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 20;

        public OrdenesProduccionController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? estado, string? busqueda, int pagina = 1)
        {
            var query = _context.OrdenesProduccion
                .Include(o => o.Pedido)
                .Include(o => o.Referencia)
                .Include(o => o.UsuarioCreador)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(o => o.Estado == estado);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(o =>
                    o.Referencia!.Codigo.Contains(busqueda) ||
                    o.PedidoId.ToString().Contains(busqueda));

            var total  = await query.CountAsync();
            var ordenes = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pagina - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Estado         = estado;
            ViewBag.Busqueda       = busqueda;
            ViewBag.PaginaActual   = pagina;
            ViewBag.TotalPaginas   = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.Total          = total;
            ViewData["ActivePage"] = "Ordenes";
            return View(ordenes);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var orden = await _context.OrdenesProduccion
                .Include(o => o.Pedido)
                    .ThenInclude(p => p!.Cliente)
                .Include(o => o.Referencia)
                .Include(o => o.UsuarioCreador)
                .Include(o => o.Procesos)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null) return NotFound();

            ViewData["ActivePage"] = "Ordenes";
            return View(orden);
        }

        [Authorize(Roles = "jefe_produccion")]
        public async Task<IActionResult> Crear(int? pedidoId)
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Referencia)
                .Where(p => p.Estado == "en_produccion")
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            ViewBag.Pedidos              = pedidos;
            ViewBag.PedidoIdSeleccionado = pedidoId;

            if (pedidoId.HasValue)
            {
                var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId.Value);
                ViewBag.PedidoSeleccionado = pedido;
            }

            ViewData["ActivePage"] = "Ordenes";
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int pedidoId,
            int referenciaId,
            int cantidadRequerida,
            bool tieneImpresion,
            bool tieneRefilado)
        {
            if (cantidadRequerida <= 0)
            {
                TempData["Error"] = "La cantidad requerida debe ser mayor a 0.";
                return RedirectToAction(nameof(Crear), new { pedidoId });
            }

            var userId = Guid.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var orden = new OrdenProduccion
            {
                PedidoId          = pedidoId,
                ReferenciaId      = referenciaId,
                CantidadRequerida = cantidadRequerida,
                Estado            = "pendiente",
                CreadoPor         = userId,
                CreatedAt         = DateTime.UtcNow
            };

            _context.OrdenesProduccion.Add(orden);
            await _context.SaveChangesAsync();

            var secuencia = 1;
            var procesos  = new List<OrdenProceso>();

            procesos.Add(new OrdenProceso
            {
                OrdenProduccionId = orden.Id,
                NombreProceso     = "extrusion",
                Secuencia         = secuencia++,
                Estado            = "pendiente"
            });

            if (tieneImpresion)
            {
                procesos.Add(new OrdenProceso
                {
                    OrdenProduccionId = orden.Id,
                    NombreProceso     = "impresion",
                    Secuencia         = secuencia++,
                    Estado            = "pendiente"
                });
            }

            if (tieneRefilado)
            {
                procesos.Add(new OrdenProceso
                {
                    OrdenProduccionId = orden.Id,
                    NombreProceso     = "refilado",
                    Secuencia         = secuencia++,
                    Estado            = "pendiente"
                });
            }

            procesos.Add(new OrdenProceso
            {
                OrdenProduccionId = orden.Id,
                NombreProceso     = "sellado",
                Secuencia         = secuencia,
                Estado            = "pendiente"
            });

            _context.OrdenProcesos.AddRange(procesos);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Orden de producción #{orden.Id} creada con {procesos.Count} procesos.";
            return RedirectToAction(nameof(Detalle), new { id = orden.Id });
        }

        [HttpPost]
        [Authorize(Roles = "jefe_produccion,operario")]
        public async Task<IActionResult> AvanzarProceso(int ordenId, int procesoId, string accion)
        {
            var proceso = await _context.OrdenProcesos
                .Include(p => p.OrdenProduccion)
                .FirstOrDefaultAsync(p => p.Id == procesoId && p.OrdenProduccionId == ordenId);

            if (proceso == null) return NotFound();

            var orden = await _context.OrdenesProduccion.FindAsync(ordenId);
            if (orden == null) return NotFound();

            if (accion == "iniciar" && proceso.Estado == "pendiente")
            {
                proceso.Estado      = "en_proceso";
                proceso.FechaInicio = DateTime.UtcNow;

                // El estado de la orden refleja el proceso activo en este momento
                orden.Estado = proceso.NombreProceso switch
                {
                    "extrusion" => "en_extrusion",
                    "impresion" => "en_impresion",
                    "refilado"  => "en_refilado",  // ← corregido
                    "sellado"   => "en_sellado",
                    _           => orden.Estado
                };
            }
            else if (accion == "completar" && proceso.Estado == "en_proceso")
            {
                proceso.Estado   = "completado";
                proceso.FechaFin = DateTime.UtcNow;

                var todosProcesos = await _context.OrdenProcesos
                    .Where(p => p.OrdenProduccionId == ordenId)
                    .ToListAsync();

                bool todosTerminados = todosProcesos.All(p =>
                    p.Estado == "completado" || p.Estado == "omitido");

                if (todosTerminados)
                {
                    orden.Estado = "completada";

                    var todasOrdenesPedido = await _context.OrdenesProduccion
                        .Where(o => o.PedidoId == orden.PedidoId)
                        .ToListAsync();

                    bool pedidoCompleto = todasOrdenesPedido.All(o =>
                        o.Estado == "completada" || o.Estado == "cancelada");

                    if (pedidoCompleto)
                    {
                        var pedido = await _context.Pedidos.FindAsync(orden.PedidoId);
                        if (pedido != null) pedido.Estado = "completado";
                    }
                }
                // Si NO todos terminaron, el estado cambia cuando se inicie el siguiente
            }
            else if (accion == "omitir" && proceso.Estado == "pendiente")
            {
                proceso.Estado = "omitido";

                var todosProcesos = await _context.OrdenProcesos
                    .Where(p => p.OrdenProduccionId == ordenId)
                    .ToListAsync();

                bool todosTerminados = todosProcesos.All(p =>
                    p.Estado == "completado" || p.Estado == "omitido");

                if (todosTerminados)
                {
                    orden.Estado = "completada";

                    var todasOrdenesPedido = await _context.OrdenesProduccion
                        .Where(o => o.PedidoId == orden.PedidoId)
                        .ToListAsync();

                    bool pedidoCompleto = todasOrdenesPedido.All(o =>
                        o.Estado == "completada" || o.Estado == "cancelada");

                    if (pedidoCompleto)
                    {
                        var pedido = await _context.Pedidos.FindAsync(orden.PedidoId);
                        if (pedido != null) pedido.Estado = "completado";
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Proceso '{proceso.NombreProceso}' actualizado.";
            return RedirectToAction(nameof(Detalle), new { id = ordenId });
        }

        [HttpGet]
        public async Task<IActionResult> ItemsPedido(int pedidoId)
        {
            var items = await _context.PedidoDetalles
                .Include(d => d.Referencia)
                .Where(d => d.PedidoId == pedidoId)
                .Select(d => new
                {
                    referenciaId   = d.ReferenciaId,
                    codigo         = d.Referencia!.Codigo,
                    nombre         = d.Referencia.Nombre,
                    cantidad       = d.Cantidad,
                    tieneImpresion = d.Referencia.Impresion,
                    tieneRefilado  = d.Referencia.RequiereRefilado
                })
                .ToListAsync();

            return Json(items);
        }
    }
}