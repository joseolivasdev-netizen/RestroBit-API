using Microsoft.AspNetCore.Mvc;
using restauranteBD.Data;
using Microsoft.EntityFrameworkCore;

namespace restauranteBD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DestinosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DestinosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDestinos()
        {
            var destinos = await _context.Destinos
                .Where(d => d.Activo)
                .Select(d => new
                {
                    idDestino = d.IdDestino,
                    nombre = d.Nombre,
                    descripcion = d.Descripcion,
                    activo = d.Activo
                })
                .ToListAsync();

            return Ok(destinos);
        }

        [HttpPost]
        public async Task<IActionResult> CrearDestino([FromBody] CrearDestinoDto dto)
        {
            var destino = new Destino
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion
            };

            _context.Destinos.Add(destino);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Destino creado correctamente", idDestino = destino.IdDestino });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarDestino(int id, [FromBody] CrearDestinoDto dto)
        {
            var destino = await _context.Destinos.FindAsync(id);
            if (destino == null)
                return NotFound(new { mensaje = "Destino no encontrado" });

            destino.Nombre = dto.Nombre;
            destino.Descripcion = dto.Descripcion;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Destino actualizado correctamente" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDestino(int id)
        {
            var destino = await _context.Destinos.FindAsync(id);
            if (destino == null)
                return NotFound(new { mensaje = "Destino no encontrado" });

            destino.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Destino desactivado correctamente" });
        }
    }

    public class CrearDestinoDto
    {
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
}
