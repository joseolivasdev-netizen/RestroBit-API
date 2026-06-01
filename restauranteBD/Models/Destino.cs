using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("destinos")]
public class Destino
{
    [Key]
    [Column("id_destino")]
    public int IdDestino { get; set; }

    [Column("nombre")]
    [Required]
    [StringLength(50)]
    public string Nombre { get; set; }

    [Column("descripcion")]
    [StringLength(255)]
    public string? Descripcion { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    // Relación con Categorias
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}