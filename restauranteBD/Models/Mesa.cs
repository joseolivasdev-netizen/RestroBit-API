using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace restauranteBD.Models
{
    [Table("mesas")]
    public class Mesa
    {
        [Key]
        [Column("id_mesa")]
        [JsonPropertyName("idMesa")]
        public int IdMesa { get; set; }

        [Column("nombre")]
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Column("tipo")]
        [StringLength(20)]
        public string? Tipo { get; set; } // "interior", "barra", "privado"

        [Column("capacidad")]
        public int? Capacidad { get; set; }

        [Column("activa")]
        public bool Activa { get; set; } = true; // Controla el color GRIS

        // Relación con Comandas para determinar ocupación
        public ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();

        // 👇 CORRECCIÓN: Ahora revisamos el nuevo EstadoPago y descartamos las canceladas
        [NotMapped]
        public bool EstaOcupada => Comandas?.Any(c => c.EstadoPago != "pagada" && c.EstadoCocina != "cancelada") ?? false;

        // 👇 CORRECCIÓN: Sumamos solo las comandas que no se han pagado ni cancelado
        [NotMapped]
        public decimal TotalAcumulado => Comandas?
            .Where(c => c.EstadoPago != "pagada" && c.EstadoCocina != "cancelada")
            .Sum(c => c.Total) ?? 0;
    }
}