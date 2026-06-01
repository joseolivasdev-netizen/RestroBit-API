namespace restauranteBD.DTOs
{
    public class ComandaDetalleResponseDto
    {
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ComandaResponseDto
    {
        public int IdComanda { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public List<ComandaDetalleResponseDto> Detalles { get; set; }
    }
}
