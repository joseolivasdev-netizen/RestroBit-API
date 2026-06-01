using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.DTOs;
using restauranteBD.Models;

namespace restauranteBD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComandasController : ControllerBase
    {
        private readonly ComandaService _service;
        private readonly AppDbContext _context;

        public ComandasController(ComandaService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearComanda([FromBody] CrearComandaDto dto)
        {
            try
            {
                // 👇 1. LÓGICA DE FOLIO DIARIO (Reinicio cada día) 👇
                var hoy = DateTime.UtcNow.Date;
                var ultimoFolio = await _context.Comandas
                    .Where(c => c.FechaApertura >= hoy)
                    .MaxAsync(c => (int?)c.FolioDiario) ?? 0;

                var usuarioId = ObtenerUsuarioIdActual();
                var comanda = await _service.CrearComandaAsync(usuarioId, dto.IdMesa, dto.NombreCliente);

                // Asignamos el nuevo folio
                comanda.FolioDiario = ultimoFolio + 1;
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetComanda), new { id = comanda.IdComanda }, comanda);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/productos")]
        public async Task<IActionResult> AgregarProducto(int id, [FromBody] AgregarProductoDto dto)
        {
            try
            {
                // 🔥 CAMBIO APLICADO AQUÍ: Se envía IdPresentacion al servicio
                await _service.AgregarProductoAsync(id, dto.IdPresentacion, dto.Cantidad, dto.Notas);

                var comanda = await _service.ObtenerComandaAsync(id);
                return Ok(comanda);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetComandas([FromQuery] string? estado = null)
        {
            var query = _context.Comandas
                .Include(c => c.Detalles)
    .ThenInclude(d => d.Presentacion)
        .ThenInclude(p => p.Producto)
            .ThenInclude(p => p.Categoria)
                .Include(c => c.Mesa)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(c => c.EstadoCocina == estado);
            }
            else
            {
                var hoy = DateTime.UtcNow.Date;
                query = query.Where(c => c.EstadoCocina != "cancelada" &&
                                     (c.EstadoPago != "pagada" || c.FechaApertura >= hoy));
            }

            var comandas = await query
                .OrderBy(c => c.FechaApertura)
                .ToListAsync();

            var resultado = comandas.Select(c => new
            {
                idComanda = c.IdComanda,
                folioDiario = c.FolioDiario, // Agregado para que Android lo lea
                nombreCliente = c.NombreCliente,
                estadoCocina = c.EstadoCocina,
                estadoPago = c.EstadoPago,
                fechaApertura = c.FechaApertura,
                segundosTranscurridos = (long)(DateTime.UtcNow - c.FechaApertura).TotalSeconds,
                mesa = c.Mesa == null ? null : new
                {
                    idMesa = c.Mesa.IdMesa,
                    nombre = c.Mesa.Nombre
                },
                total = c.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario),
                detalles = c.Detalles.Select(d => new
                {
                    idDetalle = d.IdDetalle,
                    producto = d.Presentacion?.Producto?.Nombre ?? "Producto",
                    cantidad = d.Cantidad,
                    precioUnitario = d.PrecioUnitario,
                    subtotal = d.Cantidad * d.PrecioUnitario,
                    notas = d.Notas,
                    idDestino = d.Presentacion?.Producto?.Categoria?.IdDestino ?? 0,
                    estadoItem = d.EstadoItem ?? "pendiente",

                    // Lógica de cronómetro
                    segundosPreparacion = d.FechaListo.HasValue
                    ? (long)(d.FechaListo.Value.ToUniversalTime() - c.FechaApertura.ToUniversalTime()).TotalSeconds
                    : (long)(DateTime.UtcNow - c.FechaApertura.ToUniversalTime()).TotalSeconds
                })
            });

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetComanda(int id)
        {
            try
            {
                var comanda = await _service.ObtenerComandaAsync(id);
                return Ok(comanda);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        // ==========================================
        // 👇 ENDPOINTS DE ESTADO 👇
        // ==========================================

        [HttpPatch("{id}/estado-cocina")]
        public async Task<IActionResult> CambiarEstadoCocina(int id, [FromBody] CambiarEstadoDto dto)
        {
            var comanda = await _context.Comandas.FindAsync(id);
            if (comanda == null) return NotFound();

            comanda.EstadoCocina = dto.Estado;
            await _context.SaveChangesAsync();
            return Ok(comanda);
        }

        // 💰 👇 EL ENDPOINT DEL CAJERO ACTUALIZADO 👇
        // 💰 👇 EL ENDPOINT DEL CAJERO ACTUALIZADO 👇
        [HttpPatch("{id}/estado-pago")]
        public async Task<IActionResult> CambiarEstadoPago(int id, [FromBody] CambiarEstadoDto dto)
        {
            // Usamos Include para traer los detalles, así podemos calcular el Total real
            var comanda = await _context.Comandas
                .Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdComanda == id);

            if (comanda == null) return NotFound();

            if (dto.Estado.ToLower() == "pagada")
            {
                var turnoAbierto = await _context.CortesCaja.FirstOrDefaultAsync(c => c.Estado == "abierto");

                if (turnoAbierto != null)
                {
                    comanda.IdCorte = turnoAbierto.IdCorte;

                    // 👇 NUEVO: CÁLCULO MATEMÁTICO DEL FLUJO DE EFECTIVO 👇

                    // 1. Calculamos cuánto costaba la orden originalmente sumando los detalles
                    decimal totalOrden = comanda.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

                    // 2. Restamos el descuento para obtener lo que realmente pagó el cliente
                    decimal totalPagado = totalOrden - dto.DescuentoMonto;

                    // 3. Sumamos ese dinero a la "alcancía" correspondiente del corte
                    string metodo = dto.MetodoPago?.ToLower() ?? "efectivo";
                    if (metodo == "efectivo")
                    {
                        turnoAbierto.VentasEfectivo += totalPagado;
                    }
                    else if (metodo == "tarjeta")
                    {
                        turnoAbierto.VentasTarjeta += totalPagado;
                    }

                    // 4. Registramos el descuento en los totales del día
                    turnoAbierto.TotalDescuentos += dto.DescuentoMonto;

                    // 5. Opcional: Registrar la venta total del sistema (antes de descuentos)
                    turnoAbierto.TotalVentasSistema += totalOrden;
                    // 👆 FIN DE LO NUEVO 👆
                }

                // Guardamos los datos extra para el corte
                comanda.MetodoPago = dto.MetodoPago?.ToLower() ?? "efectivo";
                comanda.DescuentoMonto = dto.DescuentoMonto;
                comanda.MotivoDescuento = dto.MotivoDescuento;
                comanda.FechaCierre = DateTime.UtcNow;
            }

            comanda.EstadoPago = dto.Estado;
            await _context.SaveChangesAsync();
            return Ok(comanda);
        }

        // ==========================================

        [HttpPatch("{id}/mesa")]
        public async Task<IActionResult> CambiarMesa(int id, [FromBody] CambiarMesaDto dto)
        {
            try
            {
                await _service.CambiarMesaAsync(id, dto.NuevaMesaId);
                var comanda = await _service.ObtenerComandaAsync(id);
                return Ok(comanda);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private int ObtenerUsuarioIdActual()
        {
            return 1; // Temporal
        }

        [HttpPatch("detalles/{idDetalle}/estado")]
        public async Task<IActionResult> CambiarEstadoDetalle(int idDetalle, [FromBody] CambiarEstadoDto dto)
        {
            // 1. Buscamos el detalle de la hamburguesa
            var detalle = await _context.DetalleComandas.FindAsync(idDetalle);
            if (detalle == null) return NotFound();

            // 2. Actualizamos el estado del producto y le ponemos hora
            detalle.EstadoItem = dto.Estado;

            // Ajusta aquí "servido", "listo" o "entregada" según la palabra exacta que mande tu app de cocina
            if (dto.Estado.ToLower() == "listo" || dto.Estado.ToLower() == "entregada" || dto.Estado.ToLower() == "servido")
            {
                detalle.FechaListo = DateTime.UtcNow;
            }

            // Guardamos el detalle primero
            await _context.SaveChangesAsync();

            // 👇 NUEVA LÓGICA: ACTUALIZAR LA COMANDA MADRE 👇

            var comandaId = detalle.IdComanda;

            // 3. Revisamos si en esta comanda aún hay productos con estado "pendiente" o "en_preparacion"
            // (Ajusta las palabras si usas otras)
            var quedanPendientes = await _context.DetalleComandas
                .AnyAsync(d => d.IdComanda == comandaId && (d.EstadoItem == "pendiente" || d.EstadoItem == "en_preparacion"));

            // 4. Si ya no queda NINGÚN pendiente, entonces la comanda entera está lista
            if (!quedanPendientes)
            {
                var comandaMadre = await _context.Comandas.FindAsync(comandaId);
                if (comandaMadre != null)
                {
                    // Forzamos el estado de la comanda a minúsculas para que Android lo lea bien
                    comandaMadre.EstadoCocina = "entregada";
                    await _context.SaveChangesAsync();
                }
            }
            // 👆 FIN DE LÓGICA NUEVA 👆

            return Ok(detalle);
        }
    }

    // --- DTOs ---
    public class CrearComandaDto
    {
        public int? IdMesa { get; set; }
        public string? NombreCliente { get; set; } = "General";
    }

    public class CambiarEstadoDto
    {
        public string Estado { get; set; }

        // Campos nuevos para que el cajero los mande
        public string? MetodoPago { get; set; } // "efectivo" o "tarjeta"
        public decimal DescuentoMonto { get; set; } = 0;
        public string? MotivoDescuento { get; set; }
    }

    public class CambiarMesaDto
    {
        public int? NuevaMesaId { get; set; }
    }

    public class AgregarProductoDto
    {
        public int IdProducto { get; set; }
        public int IdPresentacion { get; set; }
        public int Cantidad { get; set; }
        public string? Notas { get; set; }
        public string? NombreCliente { get; set; }
    }
}