using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("rollos")]
    public class Rollo
    {
        [Key][Column("id")]
        public int Id { get; set; }

        [Column("numero_rollo")]
        public string NumeroRollo { get; set; } = string.Empty;

        [Column("referencia_id")]
        public int ReferenciaId { get; set; }

        [ForeignKey("ReferenciaId")]
        public Referencia? Referencia { get; set; }

        [Column("tiene_impresion")]
        public bool TieneImpresion { get; set; } = false;

        [Column("marca_impresa")]
        public string? MarcaImpresa { get; set; }

        [Column("orden_produccion_id")]
        public int? OrdenProduccionId { get; set; }

        [ForeignKey("OrdenProduccionId")]
        public OrdenProduccion? OrdenProduccion { get; set; }

        [Column("peso_kg")]
        public decimal? PesoKg { get; set; }

        // disponible | en_proceso | usado | defectuoso
        [Column("estado")]
        public string Estado { get; set; } = "disponible";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}