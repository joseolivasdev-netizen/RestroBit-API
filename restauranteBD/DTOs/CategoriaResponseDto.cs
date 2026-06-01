namespace restauranteBD.DTOs
{
    public class CategoriaResponseDto
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
