using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace restauranteBD.DTOs
{
    public class AgregarProductoDto
    {
        [Required]
        // 1. Cambiamos el nombre que espera recibir desde el JSON de Android
        [JsonPropertyName("idPresentacion")]
        public int IdPresentacion { get; set; } // <-- Asegúrate de que diga IdPresentacion

        [Required]
        [Range(1, 100)]
        [JsonPropertyName("cantidad")] // Te sugiero usar minúsculas al inicio para el JSON
        public int Cantidad { get; set; }

        [JsonPropertyName("notas")]
        public string? Notas { get; set; }
    }
}