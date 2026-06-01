using System.ComponentModel.DataAnnotations;

namespace restauranteBD.Models
{
    public class Cuenta
    {
        [Key]
        public int IdCuenta { get; set; }
        public int? IdMesa { get; set; }
        public string Estado { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }

        public List<Comanda> Comandas { get; set; }
    }
}
