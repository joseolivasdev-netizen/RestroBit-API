using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.Models;
using restauranteBD.DTOs;

namespace restauranteBD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("GESTIÓN DE PRODUCTOS")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<IActionResult> GetProductos(string? nombre, int? categoriaId)
        {
            // 🚀 Usamos Proyección para evitar ciclos infinitos y el Error 500
            var query = _context.Productos
                .Include(p => p.Presentaciones)
                .Where(p => p.Activo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));
            }

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoriaId);
            }

            // Seleccionamos solo los campos necesarios para Android
            var productosResult = await query.Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.IdCategoria,
                p.CostoEstimado,
                p.Activo,
                // Mapeamos las presentaciones de forma limpia
                Presentaciones = p.Presentaciones.Select(pre => new
                {
                    pre.IdPresentacion,
                    pre.Nombre,
                    pre.Precio
                }).ToList()
            }).ToListAsync();

            return Ok(productosResult);
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] CrearProductoDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest(new { mensaje = "El nombre es obligatorio" });

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.IdCategoria == dto.IdCategoria);

            if (!categoriaExiste)
                return BadRequest(new { mensaje = "La categoría no existe" });

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                IdCategoria = dto.IdCategoria,
                CostoEstimado = dto.CostoEstimado,
                FechaCreacion = DateTime.UtcNow,
                Presentaciones = dto.Presentaciones.Select(p => new ProductoPresentacion
                {
                    Nombre = p.Nombre,
                    Precio = p.Precio
                }).ToList()
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Producto creado correctamente con sus variantes" });
        }

        // PUT: api/Productos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] CrearProductoDto dto)
        {
            var producto = await _context.Productos
                .Include(p => p.Presentaciones)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            producto.Nombre = dto.Nombre;
            producto.IdCategoria = dto.IdCategoria;
            producto.CostoEstimado = dto.CostoEstimado;

            // Actualización de presentaciones
            _context.ProductoPresentaciones.RemoveRange(producto.Presentaciones);
            producto.Presentaciones = dto.Presentaciones.Select(p => new ProductoPresentacion
            {
                Nombre = p.Nombre,
                Precio = p.Precio
            }).ToList();

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Producto y variantes actualizados correctamente" });
        }

        // DELETE: api/Productos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            producto.Activo = false; // Borrado lógico

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Producto desactivado correctamente" });
        }


        // GET: api/Productos/ranking
        [HttpGet("ranking")]
        public async Task<ActionResult<IEnumerable<object>>> GetRankingProductos()
        {
            // Verificamos si hay detalles de comandas
            var hayVentas = await _context.DetalleComandas.AnyAsync();

            if (!hayVentas)
            {
                // Si no hay ventas aún, devolvemos una lista vacía para no romper Django
                return Ok(new List<object>());
            }

            // Agrupamos los detalles por el nombre del producto y sumamos la cantidad
            var ranking = await _context.DetalleComandas
                .Include(dc => dc.Presentacion)
    .ThenInclude(p => p.Producto)
.GroupBy(dc => dc.Presentacion.Producto.Nombre)
                .Select(grupo => new
                {
                    Nombre = grupo.Key,
                    // Sumamos cuántas unidades se han vendido en total
                    CantidadVendida = grupo.Sum(dc => dc.Cantidad)
                })
                // Ordenamos de mayor a menor (los más vendidos primero)
                .OrderByDescending(r => r.CantidadVendida)
                .ToListAsync();

            // Mapeamos al formato JSON que espera Django (con la 'tendencia')
            var resultadoFinal = ranking.Select(r => new
            {
                nombre = r.Nombre,
                cantidad_vendida = r.CantidadVendida,
                // Lógica simple: Si vendió más de 5, tendencia arriba, sino, abajo.
                tendencia = r.CantidadVendida >= 5 ? "up" : "down"
            });

            return Ok(resultadoFinal);
        }

    }
}