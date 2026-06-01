using restauranteBD.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("productos")]
public class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("nombre")]
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [ForeignKey("IdCategoria")]
    public virtual Categoria? Categoria { get; set; }

    [Column("costo_estimado")]
    public decimal? CostoEstimado { get; set; }

    [Column("margen_ganancia")]
    public decimal? MargenGanancia { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("fecha_creacion")]
    public DateTime? FechaCreacion { get; set; }

    // ✅ Solo Presentaciones — ya no hay relación directa con DetalleComanda
    public virtual ICollection<ProductoPresentacion> Presentaciones { get; set; } = new List<ProductoPresentacion>();
}