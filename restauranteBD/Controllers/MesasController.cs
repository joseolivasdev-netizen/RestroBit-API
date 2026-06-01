using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.Models;
using restauranteBD.DTOs;

[Route("api/[controller]")]
[ApiController]
public class MesasController : ControllerBase
{
    private readonly AppDbContext _context;

    public MesasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/mesas
    [HttpGet]
    public async Task<IActionResult> GetMesas()
    {
        var mesas = await _context.Mesas
            .Include(m => m.Comandas
                // Filtramos usando los nuevos estados
                .Where(c => c.EstadoPago != "pagada" && c.EstadoCocina != "cancelada"))
                .ThenInclude(c => c.Detalles)
            .Where(m => m.Activa == true)
            .ToListAsync();

        var resultado = mesas.Select(m => new
        {
            idMesa = m.IdMesa,
            nombre = m.Nombre,
            tipo = m.Tipo,
            capacidad = m.Capacidad,
            activa = m.Activa,
            estaOcupada = m.Comandas.Any(),
            totalAcumulado = m.Comandas
                .SelectMany(c => c.Detalles)
                .Sum(d => d.Cantidad * d.PrecioUnitario),
            comandas = m.Comandas.Select(c => new
            {
                idComanda = c.IdComanda,
                // 👇 ESTA ES LA LÍNEA MÁGICA QUE FALTABA 👇
                nombreCliente = c.NombreCliente,
                estadoCocina = c.EstadoCocina,
                estadoPago = c.EstadoPago
            })
        });

        return Ok(resultado);
    }

    // GET: api/mesas/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMesa(int id)
    {
        var mesa = await _context.Mesas
            .Include(m => m.Comandas
                // Filtramos usando los nuevos estados
                .Where(c => c.EstadoPago != "pagada" && c.EstadoCocina != "cancelada"))
                .ThenInclude(c => c.Detalles)
                    .ThenInclude(d => d.Presentacion)
    .ThenInclude(p => p.Producto)
            .FirstOrDefaultAsync(m => m.IdMesa == id);

        if (mesa == null)
            return NotFound(new { error = "Mesa no encontrada" });

        var resultado = new
        {
            idMesa = mesa.IdMesa,
            nombre = mesa.Nombre,
            estaOcupada = mesa.Comandas.Any(),
            totalAcumulado = mesa.Comandas
                .SelectMany(c => c.Detalles)
                .Sum(d => d.Cantidad * d.PrecioUnitario),
            comandas = mesa.Comandas.Select(c => new
            {
                idComanda = c.IdComanda,
                nombreCliente = c.NombreCliente,
                estadoCocina = c.EstadoCocina,
                estadoPago = c.EstadoPago,
                total = c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                detalles = c.Detalles.Select(d => new
                {
                    idDetalle = d.IdDetalle,
                    producto = d.Presentacion?.Producto?.Nombre ?? "Producto",
                    cantidad = d.Cantidad,
                    precioUnitario = d.PrecioUnitario,
                    subtotal = d.Cantidad * d.PrecioUnitario,
                    notas = d.Notas
                })
            })
        };

        return Ok(resultado);
    }
}