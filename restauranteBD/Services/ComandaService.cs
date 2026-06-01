using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.DTOs;
using restauranteBD.Models;

public class ComandaService
{
    private readonly AppDbContext _context;

    public ComandaService(AppDbContext context)
    {
        _context = context;
    }

    // 1. CREAR COMANDA
    public async Task<Comanda> CrearComandaAsync(int usuarioId, int? idMesa, string nombreCliente)
    {
        var comanda = new Comanda
        {
            IdUsuario = usuarioId,
            IdMesa = idMesa == 0 ? null : idMesa,
            NombreCliente = string.IsNullOrEmpty(nombreCliente) ? "General" : nombreCliente,
            FechaApertura = DateTime.UtcNow,
            EstadoCocina = "pendiente",
            EstadoPago = "por_cobrar"
        };

        _context.Comandas.Add(comanda);
        await _context.SaveChangesAsync();
        return comanda;
    }

    // 7. LIQUIDAR CUENTA DE EMPLEADO ESPECÍFICO
    public async Task LiquidarCuentaEmpleadoAsync(int mesaId, string nombreEmpleado)
    {
        var comandasEmpleado = await _context.Comandas
            .Where(c => c.IdMesa == mesaId &&
                        c.NombreCliente == nombreEmpleado &&
                        c.EstadoPago != "pagada")
            .ToListAsync();

        if (!comandasEmpleado.Any())
            throw new Exception($"No hay cuentas activas para el empleado: {nombreEmpleado}");

        foreach (var comanda in comandasEmpleado)
        {
            comanda.EstadoPago = "pagada";
            comanda.FechaCierre = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // 2. AGREGAR PRODUCTO (Actualizado para usar IdPresentacion)
    public async Task<DetalleComanda> AgregarProductoAsync(int comandaId, int idPresentacion, int cantidad, string? notas = null)
    {
        try
        {
            Console.WriteLine($"Buscando presentación con ID: {idPresentacion}");

            // 1. Validar comanda y estados
            var comanda = await _context.Comandas.FindAsync(comandaId);
            if (comanda == null || comanda.EstadoPago == "pagada" || comanda.EstadoCocina == "cancelada")
                throw new Exception("Comanda no válida o cerrada");

            // 2. BUSCAR LA PRESENTACIÓN Y SU PRECIO REAL
            // Cambiamos ".Presentaciones" por el nombre real de tu DbSet
            var presentacion = await _context.ProductoPresentaciones
                .Include(p => p.Producto)
                .FirstOrDefaultAsync(p => p.IdPresentacion == idPresentacion);

            if (presentacion == null) throw new Exception("Presentación no encontrada en la base de datos");

            decimal precioFinal = presentacion.Precio;

            // 3. Crear detalle apuntando a la presentación
            var detalle = new DetalleComanda
            {
                IdComanda = comandaId,
                IdPresentacion = idPresentacion,
                Cantidad = cantidad,
                PrecioUnitario = precioFinal,
                Notas = notas ?? string.Empty,
                EstadoItem = "pendiente"
            };

            _context.DetalleComandas.Add(detalle);
            await _context.SaveChangesAsync();

            return detalle;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al agregar detalle: {ex.Message}");
            throw;
        }
    }

    // 3. OBTENER COMANDA (Actualizado para navegar por Presentacion -> Producto)
    public async Task<ComandaResponseDto> ObtenerComandaAsync(int id)
    {
        var comanda = await _context.Comandas
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Presentacion)
                    .ThenInclude(p => p.Producto)
            .FirstOrDefaultAsync(c => c.IdComanda == id);

        if (comanda == null)
            throw new Exception("Comanda no encontrada");

        var total = comanda.Detalles?.Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0;

        return new ComandaResponseDto
        {
            IdComanda = comanda.IdComanda,
            Total = total,
            Estado = comanda.EstadoPago,
            Detalles = comanda.Detalles?.Select(d => new ComandaDetalleResponseDto
            {
                // Obtenemos el nombre del producto navegando a través de la presentación
                Producto = d.Presentacion?.Producto?.Nombre ?? "Producto",
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList() ?? new List<ComandaDetalleResponseDto>()
        };
    }

    // 4. CAMBIAR MESA 
    public async Task CambiarMesaAsync(int comandaId, int? nuevaMesaId)
    {
        var comanda = await _context.Comandas
            .FirstOrDefaultAsync(c => c.IdComanda == comandaId
                && c.EstadoPago != "pagada"
                && c.EstadoCocina != "cancelada");

        if (comanda == null)
            throw new Exception("Comanda no encontrada o no se puede mover");

        comanda.IdMesa = nuevaMesaId;
        await _context.SaveChangesAsync();
    }

    // 6. OBTENER MESAS CON ESTADO 
    public async Task<List<MesaEstadoDto>> ObtenerEstadoMesasAsync()
    {
        var mesas = await _context.Mesas
            .Include(m => m.Comandas.Where(c => c.EstadoPago != "pagada" && c.EstadoCocina != "cancelada"))
                .ThenInclude(c => c.Detalles)
            .ToListAsync();

        return mesas.Select(m => new MesaEstadoDto
        {
            IdMesa = m.IdMesa,
            Nombre = m.Nombre,
            ComandasActivas = m.Comandas?.Count ?? 0,
            TotalMesa = m.Comandas?
                    .SelectMany(c => c.Detalles ?? new List<DetalleComanda>())
                    .Sum(d => d.Cantidad * d.PrecioUnitario) ?? 0
        }).ToList();
    }
}

// DTO auxiliar para el estado de mesas
public class MesaEstadoDto
{
    public int IdMesa { get; set; }
    public string Nombre { get; set; }
    public int ComandasActivas { get; set; }
    public decimal TotalMesa { get; set; }
}