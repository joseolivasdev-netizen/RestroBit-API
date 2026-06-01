using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace restauranteBD.Models
{
    [Table("producto_presentaciones")]
    public class ProductoPresentacion
    {
        [Key]
        [Column("id_presentacion")]
        public int IdPresentacion { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("nombre_presentacion")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } // Ejemplo: "12oz", "16oz"

        [Column("precio")]
        public decimal Precio { get; set; } // Ejemplo: 45.00

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }
    }
}