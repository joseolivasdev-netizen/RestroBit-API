using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("categorias")]
public class Categoria
{
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Column("nombre")]
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string? Tipo { get; set; } // Ej: "comida", "bebida", "postre"

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    // ← Nuevo: relación con Destino
    [Column("id_destino")]
    public int? IdDestino { get; set; }

    [ForeignKey("IdDestino")]
    public Destino? Destino { get; set; }

    // ✅ NUEVO: Relación con Productos
    /// <summary>
    /// Productos que pertenecen a esta categoría
    /// </summary>
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}