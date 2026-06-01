using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using restauranteBD.Models;

[Table("comandas")]
public class Comanda
{
    [Column("id_cuenta")] // 👈 Forzamos el nombre exacto de la DB
    public int? IdCuenta { get; set; } // Ponlo como nullable si en la DB puede ser null

    [ForeignKey("IdCuenta")] // 👈 Vinculamos la navegación con la columna de arriba
    public Cuenta? Cuenta { get; set; }
    [Key]
    [Column("id_comanda")]
    public int IdComanda { get; set; }

    [Column("id_usuario")]
    [Required]
    public int IdUsuario { get; set; } // Mesero que tomó el pedido

    [Column("id_mesa")]
    public int? IdMesa { get; set; } // AHORA ES NULLABLE (para llevar/barra)

    [Column("fecha_apertura")]
    public DateTime FechaApertura { get; set; }

    [Column("fecha_cierre")]
    public DateTime? FechaCierre { get; set; }
    // ... tus otras propiedades ...

    [Column("id_corte")]
    public int? IdCorte { get; set; } // Es nulable (?) porque cuando el mesero la crea, aún no se sabe cuándo ni en qué turno se pagará

    // 👇 NUEVAS COLUMNAS PARA EL CAJERO Y FOLIOS 👇

    [Column("folio_diario")]
    public int? FolioDiario { get; set; } = 0;

    [Column("metodo_pago")]
    public string? MetodoPago { get; set; }

    [Column("descuento_monto")]
    public decimal? DescuentoMonto { get; set; } 

    [Column("motivo_descuento")]
    public string? MotivoDescuento { get; set; }

   


    // --- ESTADOS SEPARADOS ---

    [Column("estado_cocina")]
    [Required]
    [StringLength(20)]
    // Valores: pendiente, en_preparacion, entregada
    public string EstadoCocina { get; set; } = "pendiente";

    [Column("estado_pago")]
    [Required]
    [StringLength(20)]
    // Valores: por_cobrar, pagada
    public string EstadoPago { get; set; } = "por_cobrar";

    // --- PROPIEDADES CALCULADAS ACTUALIZADAS ---

    [NotMapped]
    public bool EstaPagada => EstadoPago == "pagada";

    [NotMapped]
    // Una comanda está "Activa" si no se ha pagado O si la comida no se ha entregado
    public bool EstaActiva => EstadoPago != "pagada" || EstadoCocina != "entregada";

    // Relaciones
    [ForeignKey("IdMesa")]
    public Mesa? Mesa { get; set; } // nullable porque IdMesa ahora es nullable

    [ForeignKey("IdUsuario")]
    public Usuario Usuario { get; set; } // Asumiendo que tienes modelo Usuario

    public ICollection<DetalleComanda> Detalles { get; set; } = new List<DetalleComanda>();

    // 👇 PROPIEDADES CALCULADAS (NO se guardan en BD)
    [NotMapped]
    public decimal Total => Detalles?.Sum(d => d.Subtotal) ?? 0;

    [NotMapped]
    public int TotalItems => Detalles?.Sum(d => d.Cantidad) ?? 0;

 

    [Column("nombre_cliente")] // Asegúrate de agregar esta columna en tu base de datos PostgreSQL
    [StringLength(100)]
    public string NombreCliente { get; set; } = "General"; // Aquí guardaremos el nombre del empleado
}