using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Models;

namespace PlastiPack.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Referencia> Referencias { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<PrecioReferencia> PreciosReferencia { get; set; }
        public DbSet<Rollo> Rollos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<PedidoHistorialEstado> PedidoHistorial { get; set; }
        public DbSet<OrdenProduccion> OrdenesProduccion { get; set; }
        public DbSet<OrdenProceso> OrdenProcesos { get; set; }
        public DbSet<Selladora> Selladoras { get; set; }
        public DbSet<Planilla> Planillas { get; set; }
        public DbSet<PlanillaItem> PlanillaItems { get; set; }
        public DbSet<RegistroSellado> RegistrosSellado { get; set; }
    }
}
