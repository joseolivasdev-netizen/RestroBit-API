using restauranteBD.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("detalle_comanda")]
public class DetalleComanda
{
    [Key]
    [Column("id_detalle")]
    public int IdDetalle { get; set; }

    [Column("id_comanda")]
    public int IdComanda { get; set; }

    [ForeignKey("IdComanda")]
    public Comanda Comanda { get; set; }

    // 👇 1. CAMBIO CLAVE: Nombre correcto de la variable
    [Column("id_presentacion")]
    public int IdPresentacion { get; set; }

    // 👇 2. CAMBIO CLAVE: Apuntamos al modelo correcto (Asegúrate de que tu clase se llame ProductoPresentacion)
    [ForeignKey("IdPresentacion")]
    public ProductoPresentacion Presentacion { get; set; }

    [Column("cantidad")]
    [Required]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [Column("notas")]
    [StringLength(200)]
    public string? Notas { get; set; } // "sin cebolla", "punto medio", etc.

    [Column("estado_item")]
    [StringLength(20)]
    public string EstadoItem { get; set; } = "pendiente"; // pendiente, servido, cancelado

    [Column("fecha_listo")]
    public DateTime? FechaListo { get; set; }

    // 👇 PROPIEDAD CALCULADA - NO se guarda en BD
    [NotMapped]
    public decimal Subtotal => Cantidad * PrecioUnitario;

    // Constructor para facilitar creación
    public DetalleComanda()
    {
    }

    // 👇 3. CAMBIO CLAVE: El constructor ahora recibe idPresentacion
    public DetalleComanda(int idPresentacion, int cantidad, decimal precioUnitario, string? notas = null)
    {
        IdPresentacion = idPresentacion;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Notas = notas;
    }
}