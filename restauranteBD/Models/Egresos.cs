using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



[Table("egresos")]
public class Egreso
{
    [Key]
    [Column("id_egreso")]
    public int IdEgreso { get; set; }

    // Relación con el Corte de Caja actual
    [Column("id_corte")]
    public int IdCorte { get; set; }

    [Required]
    // Nota cómo "monto" (con minúscula) es el primer parámetro de [Column]
    [Column("monto", TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Required]
    [StringLength(255)]
    [Column("concepto")]
    public string Concepto { get; set; }

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}