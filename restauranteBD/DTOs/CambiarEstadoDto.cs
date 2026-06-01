using System.ComponentModel.DataAnnotations;

namespace restauranteBD.DTOs
{
    // Para cambiar estado (usando PATCH)
    public class CambiarEstadoDto
    {
        [Required]
        [RegularExpression("^(activa|pagada|cancelada)$",
            ErrorMessage = "Estado debe ser 'activa', 'pagada' o 'cancelada'")]
        public string Estado { get; set; }
    }

    // Para cambiar mesa (nuevo endpoint que sugiero)
    public class CambiarMesaDto
    {
        public int? NuevaMesaId { get; set; } // nullable para poder quitar mesa (para llevar)
    }

    // Para cuando listas mesas con estado
    public class MesaEstadoDto
    {
        public int IdMesa { get; set; }
        public string Nombre { get; set; }
        public int ComandasActivas { get; set; }
        public decimal TotalMesa { get; set; }
        public bool EstaOcupada => ComandasActivas > 0;
        public bool Activa { get; set; } // Si es false, en Android pintas Gris.
    }
}
