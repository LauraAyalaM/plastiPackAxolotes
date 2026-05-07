using Microsoft.EntityFrameworkCore;
using PlastiPack.API.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Converter para DateTime
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc
                    ? v
                    : DateTime.SpecifyKind(v, DateTimeKind.Utc),

                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            // Converter para DateTime?
            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue
                    ? (v.Value.Kind == DateTimeKind.Utc
                        ? v
                        : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
                    : v,

                v => v.HasValue
                    ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)
                    : v
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeProperties = entityType.ClrType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime));

                foreach (var property in dateTimeProperties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(dateTimeConverter);
                }

                var nullableDateTimeProperties = entityType.ClrType
                    .GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime?));

                foreach (var property in nullableDateTimeProperties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(nullableDateTimeConverter);
                }
            }
        }
    }
}