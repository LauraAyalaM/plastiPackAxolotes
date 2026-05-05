using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("orden_procesos")]
    public class OrdenProceso
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("orden_produccion_id")]
        public int OrdenProduccionId { get; set; }

        [ForeignKey("OrdenProduccionId")]
        public OrdenProduccion? OrdenProduccion { get; set; }

        [Column("nombre_proceso")]
        public string NombreProceso { get; set; } = string.Empty; // 'extrusion', 'impresion', 'refilado', 'sellado'

        [Column("secuencia")]
        public int Secuencia { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("fecha_inicio")]
        public DateTime? FechaInicio { get; set; }

        [Column("fecha_fin")]
        public DateTime? FechaFin { get; set; }
    }
}
