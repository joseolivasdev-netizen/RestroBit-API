using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.DTOs;
using Swashbuckle.AspNetCore.Annotations; 

namespace restauranteBD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag(" Roles del Sistema")] 
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los roles del sistema
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Devuelve la lista completa de roles disponibles en el restaurante.
        /// 
        /// **¿Para qué sirve?**
        /// - Para mostrar en formularios de registro de usuarios
        /// - Para llenar combos de selección de roles
        /// - Para administración de permisos
        /// 
        /// **Ejemplo de respuesta exitosa:**
        /// 
        ///     [
        ///       {
        ///         "idRol": 1,
        ///         "nombre": "administrador",
        ///         "descripcion": "acceso completo al sistema"
        ///       },
        ///       {
        ///         "idRol": 2,
        ///         "nombre": "servicio",
        ///         "descripcion": "acceso sistema de comandas"
        ///       }
        ///     
        ///     ]
        /// 
        /// **Posibles errores:**
        /// - 500: Error interno del servidor
        /// </remarks>
        /// <returns>Lista de roles</returns>
        /// <response code="200"> Éxito - Devuelve lista de roles</response>
        /// <response code="500"> Error - Problema con la base de datos</response>
        [HttpGet] // GET: api/roles
        [ProducesResponseType(typeof(List<RolResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Select(r => new RolResponseDto
                    {
                        IdRol = r.IdRol,
                        Nombre = r.Nombre,
                        Descripcion = r.Descripcion
                    })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                // Log del error
                return StatusCode(500, new { mensaje = "Error al obtener roles", error = ex.Message });
            }
        }
    }
}