using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.Models;

namespace restauranteBD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CortesCajaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CortesCajaController(AppDbContext context)
        {
            _context = context;
        }

        // 1. ENDPOINT PARA ABRIR LA CAJA
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaRequest request)
        {
            // Creamos el registro real en la base de datos
            var nuevoCorte = new CorteCaja
            {
                IdUsuario = request.IdUsuario,
                FondoInicial = (decimal)request.FondoInicial, // Convertimos el double a decimal para BD
                Estado = "abierto",
                FechaApertura = DateTime.UtcNow
            };

            _context.CortesCaja.Add(nuevoCorte);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Caja abierta con éxito" });
        }

        // 2. ENDPOINT PARA VERIFICAR SI LA CAJA ESTÁ ABIERTA
        [HttpGet("actual")]
        public async Task<IActionResult> GetTurnoActual()
        {
            // Buscamos si existe algún corte con estado "abierto"
            var turnoAbierto = await _context.CortesCaja
                .FirstOrDefaultAsync(c => c.Estado == "abierto");

            if (turnoAbierto == null)
            {
                // Esto es lo que la tablet espera para mostrar "Caja Cerrada"
                return NotFound(new { mensaje = "No hay turno abierto actualmente." });
            }

            // Si hay turno, lo devolvemos y la tablet quita la pantalla de "Abrir caja"
            return Ok(turnoAbierto);
        }
        // POST: api/cortescaja/egresos
        [HttpPost("egresos")]
        public async Task<IActionResult> RegistrarEgreso([FromBody] Egreso egreso)
        {
            var turnoAbierto = await _context.CortesCaja
                .FirstOrDefaultAsync(c => c.Estado == "abierto");

            if (turnoAbierto == null)
            {
                return BadRequest(new { mensaje = "No hay un turno abierto para registrar egresos." });
            }

            // Asignamos el egreso al turno actual
            egreso.IdCorte = turnoAbierto.IdCorte;

            _context.Egresos.Add(egreso);

            // Actualizamos el total de gastos del turno
            turnoAbierto.TotalGastos += egreso.Monto;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Egreso registrado correctamente." });
        }

        // GET: api/cortescaja/egresos
        [HttpGet("egresos")]
        public async Task<IActionResult> ObtenerEgresosActuales()
        {
            var turnoAbierto = await _context.CortesCaja
                .FirstOrDefaultAsync(c => c.Estado == "abierto");

            if (turnoAbierto == null)
                return Ok(new List<Egreso>());

            var egresos = await _context.Egresos
                .Where(e => e.IdCorte == turnoAbierto.IdCorte)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();

            return Ok(egresos);
        }

        [HttpPatch("{id}/cerrar")]
        public async Task<IActionResult> CerrarTurno(int id, [FromBody] CerrarTurnoDto dto)
        {
            var corte = await _context.CortesCaja.FindAsync(id);
            if (corte == null || corte.Estado == "cerrado") return NotFound();

            // Sumar el total contado por el cajero
            corte.TotalArqueoFisico = dto.EfectivoFisico + dto.TarjetaFisica;

            // Lo que el sistema esperaba en total
            decimal esperadoTotal = corte.FondoInicial + corte.VentasEfectivo + corte.VentasTarjeta - corte.TotalGastos;

            // Calcular si sobra o falta dinero
            corte.Diferencia = corte.TotalArqueoFisico - esperadoTotal;

            corte.FechaCierre = DateTime.UtcNow;
            corte.Estado = "cerrado";

            await _context.SaveChangesAsync();
            return Ok(corte);
        }

        // Necesitas este DTO en el mismo archivo o en tu carpeta DTOs
        public class CerrarTurnoDto
        {
            public decimal EfectivoFisico { get; set; }
            public decimal TarjetaFisica { get; set; }
        }


    }
}