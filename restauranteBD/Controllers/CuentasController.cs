using Microsoft.AspNetCore.Mvc;
using restauranteBD.Data;
using restauranteBD.Models;
using Microsoft.EntityFrameworkCore;

namespace restauranteBD.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CuentasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearCuenta([FromBody] Cuenta cuenta)
        {
            _context.Cuentas.Add(cuenta);
            await _context.SaveChangesAsync();

            return Ok(cuenta);
        }

        [HttpGet("abiertas")]
        public async Task<IActionResult> ObtenerCuentasAbiertas()
        {
            var cuentas = await _context.Cuentas
                .Where(c => c.Estado == "abierta")
                .ToListAsync();

            return Ok(cuentas);
        }

        [HttpGet("mesa/{idMesa}")]
        public async Task<IActionResult> ObtenerCuentaPorMesa(int idMesa)
        {
            var cuenta = await _context.Cuentas
                .Where(c => c.IdMesa == idMesa && c.Estado == "abierta")
                .FirstOrDefaultAsync();

            return Ok(cuenta);
        }

        // 👇 CORRECCIÓN 1: Agregamos :int a las rutas para evitar el Error 400
        [HttpPost("{idCuenta:int}/cerrar")]
        public async Task<IActionResult> CerrarCuenta(int idCuenta)
        {
            var cuenta = await _context.Cuentas.FindAsync(idCuenta);

            if (cuenta == null)
                return NotFound();

            cuenta.Estado = "cerrada";
            cuenta.FechaCierre = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(cuenta);
        }

        [HttpPost("{idCuenta:int}/pagar")]
        public async Task<IActionResult> PagarCuenta(int idCuenta)
        {
            var cuenta = await _context.Cuentas.FindAsync(idCuenta);

            if (cuenta == null)
                return NotFound();

            cuenta.Estado = "pagada";

            await _context.SaveChangesAsync();

            return Ok(cuenta);
        }

        [HttpGet("{idCuenta:int}")]
        public async Task<IActionResult> ObtenerCuentaPorId(int idCuenta)
        {
            var cuenta = await _context.Cuentas
                .Where(c => c.IdCuenta == idCuenta)
                .Select(c => new
                {
                    c.IdCuenta,
                    c.IdMesa,
                    c.Estado,
                    c.FechaApertura,
                    c.FechaCierre,

                    Comandas = _context.Comandas
                        .Where(cmd => cmd.IdCuenta == c.IdCuenta)
                        .Select(cmd => new
                        {
                            cmd.IdComanda,
                            EstadoCocina = cmd.EstadoCocina,
                            EstadoPago = cmd.EstadoPago,

                            Detalles = _context.DetalleComandas
                                .Where(d => d.IdComanda == cmd.IdComanda)
                                .Select(d => new
                                {
                                    d.IdDetalle,
                                    d.IdPresentacion,
                                    d.Cantidad,
                                    d.PrecioUnitario,
                                    d.EstadoItem,
                                    d.Notas
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (cuenta == null)
                return NotFound();

            return Ok(cuenta);
        }

        // 👇 CORRECCIÓN 2: Buscamos directamente en Comandas para que detecte nuestra simulación
        [HttpGet("resumen-financiero")]
        public async Task<IActionResult> ObtenerResumenFinanciero()
        {
            try
            {
                // 1. Buscamos directamente las COMANDAS que ya están pagadas
                var comandasPagadasIds = await _context.Comandas
                    .Where(cmd => cmd.EstadoPago == "pagado" || cmd.EstadoPago == "pagada")
                    .Select(cmd => cmd.IdComanda)
                    .ToListAsync();

                if (!comandasPagadasIds.Any())
                {
                    return Ok(new { ganancias_totales = 0, ingreso_semanal = 0, costo_ventas = 0, utilidad_neta = 0 });
                }

                // 2. Calculamos los INGRESOS sumando los detalles de esas comandas
                var ingresosTotales = await _context.DetalleComandas
                    .Where(d => comandasPagadasIds.Contains((int)d.IdComanda))
                    .SumAsync(d => d.Cantidad * d.PrecioUnitario);

                // 3. Calculamos los COSTOS uniendo con la tabla Productos
                var costosTotales = await _context.DetalleComandas
                    .Where(d => comandasPagadasIds.Contains((int)d.IdComanda))
                    .Join(_context.Productos,
                          detalle => detalle.IdPresentacion,
                          producto => producto.IdProducto,
                          (detalle, producto) => detalle.Cantidad * producto.CostoEstimado)
                    .SumAsync();

                // 4. Calculamos la UTILIDAD NETA
                var utilidadNeta = ingresosTotales - costosTotales;

                return Ok(new
                {
                    ganancias_totales = ingresosTotales,
                    ingreso_semanal = ingresosTotales,
                    costo_ventas = costosTotales,
                    utilidad_neta = utilidadNeta
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al calcular finanzas", detalle = ex.Message });
            }
        }

        [HttpGet("ventas-por-categoria")]
        public async Task<IActionResult> ObtenerVentasPorCategoria()
        {
            try
            {
                // Solo tomamos en cuenta comandas que ya se pagaron
                var comandasPagadas = await _context.Comandas
                    .Where(c => c.EstadoPago == "pagado" || c.EstadoPago == "pagada")
                    .Select(c => c.IdComanda)
                    .ToListAsync();

                var ventasPorCategoria = await _context.DetalleComandas
                    .Where(d => comandasPagadas.Contains((int)d.IdComanda))
                    .Join(_context.Productos, d => d.IdPresentacion, p => p.IdProducto, (d, p) => new { d.Cantidad, p.IdCategoria })
                    .Join(_context.Categorias, dp => dp.IdCategoria, c => c.IdCategoria, (dp, c) => new { c.Nombre, dp.Cantidad })
                    .GroupBy(x => x.Nombre)
                    .Select(g => new {
                        categoria = g.Key,
                        cantidad = g.Sum(x => x.Cantidad)
                    })
                    .OrderByDescending(x => x.cantidad)
                    .ToListAsync();

                return Ok(ventasPorCategoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al agrupar categorías", detalle = ex.Message });
            }
        }
    }
}