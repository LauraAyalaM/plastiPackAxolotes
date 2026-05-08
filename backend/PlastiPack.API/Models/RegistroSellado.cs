using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("registros_sellado")]
    public class RegistroSellado
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("planilla_item_id")]
        public int PlanillaItemId { get; set; }

        [ForeignKey("PlanillaItemId")]
        public PlanillaItem? PlanillaItem { get; set; }

        [Column("operario_id")]
        public Guid OperarioId { get; set; }

        [ForeignKey("OperarioId")]
        public Usuario? Operario { get; set; }

        // ← Reemplaza RolloId/Rollo por texto libre
        [Column("numero_rollo")]
        public string? NumeroRollo { get; set; }

        [Column("hora_inicio")]
        public TimeOnly HoraInicio { get; set; }

        [Column("hora_fin")]
        public TimeOnly? HoraFin { get; set; }

        [Column("cantidad_unidades")]
        public int? CantidadUnidades { get; set; }

        [Column("peso_desperdicio")]
        public decimal PesoDesperdicio { get; set; } = 0;

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}