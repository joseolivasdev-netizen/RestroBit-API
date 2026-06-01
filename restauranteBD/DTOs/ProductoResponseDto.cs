namespace restauranteBD.DTOs
{
    public class ProductoResponseDto
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal PrecioVenta { get; set; }
    }
}
