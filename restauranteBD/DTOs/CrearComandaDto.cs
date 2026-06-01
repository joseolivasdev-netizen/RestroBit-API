using System.Text.Json.Serialization;

namespace restauranteBD.DTOs
{
    public class CrearComandaDto
    {
        [JsonPropertyName("IdMesa")]
        public int? IdMesa { get; set; }

        [JsonPropertyName("NombreCliente")]
        public string? NombreCliente { get; set; } // <--- ¡Faltaba esta línea!

        public string? TipoOrigen { get; set; }
    }
}