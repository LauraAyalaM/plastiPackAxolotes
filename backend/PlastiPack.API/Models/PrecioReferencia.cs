using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("precios_referencia")]
    public class PrecioReferencia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("referencia_id")]
        public int ReferenciaId { get; set; }

        [ForeignKey("ReferenciaId")]
        public Referencia? Referencia { get; set; }

        [Column("categoria")]
        public string Categoria { get; set; } = string.Empty; // 'Mayorista', 'Mostrador', 'Lista'

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("vigente_desde")]
        public DateTime VigenteDesde { get; set; } = DateTime.UtcNow;
    }
}
