using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlastiPack.API.Models
{
    [Table("referencias")]
    public class Referencia
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("referencia_corta")]
        public string? ReferenciCorta { get; set; }

        [Column("nombre")]
        public string? Nombre { get; set; }

        [Column("grupo")]
        public string? Grupo { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "Activo";

        [Column("tipo_producto")]
        public string? TipoProducto { get; set; }

        [Column("materia_prima")]
        public string? MateriaPrima { get; set; }

        [Column("color")]
        public string? Color { get; set; }

        [Column("troquelado")]
        public string? Troquelado { get; set; }

        [Column("ancho")]
        public decimal? Ancho { get; set; }

        [Column("fuelle_izquierdo")]
        public decimal? FuelleIzquierdo { get; set; }

        [Column("fuelle_derecho")]
        public decimal? FuelleDerecho { get; set; }

        [Column("alto")]
        public decimal? Alto { get; set; }

        [Column("fuelle_superior")]
        public decimal? FuelleSuperior { get; set; }

        [Column("fuelle_fondo")]
        public decimal? FuelleFondo { get; set; }

        [Column("calibre")]
        public decimal? Calibre { get; set; }

        [Column("impresion")]
        public bool Impresion { get; set; } = false;

        [Column("colores_impresion")]
        public string? ColoresImpresion { get; set; }

        [Column("tipo_cliente")]
        public string? TipoCliente { get; set; }

        [Column("tipo_impresion")]
        public string? TipoImpresion { get; set; }

        [Column("tipo_sellado")]
        public string? TipoSellado { get; set; }

        [Column("tratado_cara")]
        public string? TratadoCara { get; set; }

        [Column("medida")]
        public string? Medida { get; set; }

        [Column("costo_produccion")]
        public decimal? CostoProduccion { get; set; }

        [Column("impuesto")]
        public decimal? Impuesto { get; set; }

        [Column("codigo_barras")]
        public string? CodigoBarras { get; set; }

        [Column("presentacion")]
        public string? Presentacion { get; set; }

        [Column("unidad_medida")]
        public string? UnidadMedida { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("creado_por")]
        public Guid? CreadoPor { get; set; }

        [ForeignKey("CreadoPor")]
        public Usuario? UsuarioCreador { get; set; }

        public Inventario? Inventario { get; set; }
        public ICollection<PrecioReferencia> Precios { get; set; } = new List<PrecioReferencia>();
    }
}
