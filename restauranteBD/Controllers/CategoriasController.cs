using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.Models;
using restauranteBD.DTOs;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("GESTIÓN DE CATEGORÍAS")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/categorias
    [HttpGet]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await _context.Categorias
            .Include(c => c.Destino)
            .Where(c => c.Activo)
            .Select(c => new
            {
                idCategoria = c.IdCategoria,
                nombre = c.Nombre,
                tipo = c.Tipo,
                descripcion = c.Descripcion,
                activo = c.Activo,
                destino = c.Destino == null ? null : new
                {
                    idDestino = c.Destino.IdDestino,
                    nombre = c.Destino.Nombre
                }
            })
            .ToListAsync();

        return Ok(categorias);
    }

    // GET: api/categorias/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoria(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Destino)
            .Where(c => c.IdCategoria == id)
            .Select(c => new
            {
                idCategoria = c.IdCategoria,
                nombre = c.Nombre,
                tipo = c.Tipo,
                descripcion = c.Descripcion,
                activo = c.Activo,
                destino = c.Destino == null ? null : new
                {
                    idDestino = c.Destino.IdDestino,
                    nombre = c.Destino.Nombre
                }
            })
            .FirstOrDefaultAsync();

        if (categoria == null)
            return NotFound(new { mensaje = "Categoría no encontrada" });

        return Ok(categoria);
    }

    // POST: api/categorias
    [HttpPost]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { mensaje = "El nombre es obligatorio" });

        var existe = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower());
        if (existe)
            return BadRequest(new { mensaje = "Ya existe una categoría con ese nombre" });

        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion,
            IdDestino = dto.IdDestino  // ← nuevo
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Categoría creada correctamente" });
    }

    // PUT: api/categorias/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] ActualizarCategoriaDto dto)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound(new { mensaje = "Categoría no encontrada" });

        categoria.Nombre = dto.Nombre;
        categoria.Tipo = dto.Tipo;
        categoria.Descripcion = dto.Descripcion;
        categoria.IdDestino = dto.IdDestino;  // ← nuevo

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Categoría actualizada correctamente" });
    }

    // DELETE: api/categorias/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound(new { mensaje = "Categoría no encontrada" });

        categoria.Activo = false;
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Categoría desactivada correctamente" });
    }
}

// DTOs
public class CrearCategoriaDto
{
    public string Nombre { get; set; }
    public string? Tipo { get; set; }
    public string? Descripcion { get; set; }
    public int? IdDestino { get; set; }  // ← nuevo
}

public class ActualizarCategoriaDto
{
    public string Nombre { get; set; }
    public string? Tipo { get; set; }
    public string? Descripcion { get; set; }
    public int? IdDestino { get; set; }  // ← nuevo
}
