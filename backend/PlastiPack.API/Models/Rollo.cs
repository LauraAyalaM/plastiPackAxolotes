using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("rollos")]
    public class Rollo
    {
        [Key]
        [Column("id")]
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

        [Column("peso_kg")]
        public decimal? PesoKg { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "disponible"; // 'disponible', 'en_proceso', 'usado', 'defectuoso'

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
