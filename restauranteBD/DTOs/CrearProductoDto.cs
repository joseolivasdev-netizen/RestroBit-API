using System.ComponentModel.DataAnnotations;

namespace restauranteBD.DTOs
{
    public class CrearProductoDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int IdCategoria { get; set; }

        public decimal? CostoEstimado { get; set; }

        // ✅ REEMPLAZO: En lugar de PrecioVenta, usamos una lista de variantes
        public List<PresentacionDto> Presentaciones { get; set; } = new List<PresentacionDto>();
    }

    public class PresentacionDto
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty; // Ej: "12oz", "16oz", "Pieza"

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; } // Ej: 45.00, 65.00
    }
}