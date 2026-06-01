using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restauranteBD.Data;
using restauranteBD.Models;
using restauranteBD.DTOs;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations; 

namespace restauranteBD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag(" GESTIÓN DE USUARIOS - Administración de usuarios del sistema")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        ///  OBTENER TODOS LOS USUARIOS
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Devuelve la lista completa de usuarios registrados en el sistema.
        ///
        /// ** Requiere autenticación:**
        /// * Rol requerido: **administrador**
        /// * Token JWT válido con rol "administrador"
        /// 
        /// **¿Para qué sirve?**
        /// - Para administración de usuarios
        /// - Para ver quiénes tienen acceso al sistema
        /// - Para monitorear usuarios activos/inactivos
        /// 
        /// **Ejemplo de respuesta exitosa:**
        /// 
        ///     [
        ///       {
        ///         "idUsuario": 1,
        ///         "nombre": "admin",
        ///         "idRol": 1,
        ///         "activo": true
        ///       },
        ///       {
        ///         "idUsuario": 2,
        ///         "nombre": "juan",
        ///         "idRol": 2,
        ///         "activo": true
        ///       }
        ///     ]
        /// 
        /// **Posibles errores:**
        /// * 401: No autenticado (falta token)
        /// * 403: No autorizado (no es administrador)
        /// * 500: Error interno del servidor
        /// </remarks>
        /// <returns>Lista de usuarios</returns>
        /// <response code="200"> Éxito - Lista de usuarios</response>
        /// <response code="401"> No autenticado - Token requerido</response>
        /// <response code="403"> No autorizado - Se requiere rol administrador</response>
        /// <response code="500"> Error interno</response>
        [Authorize(Roles = "administrador")]
        [HttpGet]
        [ProducesResponseType(typeof(List<UsuarioResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Select(u => new UsuarioResponseDto
                    {
                        IdUsuario = u.IdUsuario,
                        Nombre = u.Nombre,
                        IdRol = u.IdRol,
                        Activo = u.Activo
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuarios", error = ex.Message });
            }
        }

        /// <summary>
        ///  CREAR NUEVO USUARIO
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Crea un nuevo usuario en el sistema con contraseña encriptada.
        ///
        /// ** Requiere autenticación:**
        /// * Rol requerido: **administrador**
        /// 
        /// ** Reglas de validación:**
        /// * El nombre de usuario debe ser único
        /// * La contraseña se guarda encriptada con BCrypt
        /// * El rol debe existir en la base de datos
        /// * El usuario se crea activo por defecto
        /// 
        /// ** Ejemplo de petición:**
        /// 
        ///     POST /api/usuarios
        ///     {
        ///         "nombre": "nuevo_usuario",
        ///         "password": "MiPassword123",
        ///         "idRol": 2
        ///     }
        /// 
        /// **Campos requeridos:**
        /// * **nombre**: Mínimo 3 caracteres
        /// * **password**: Mínimo 6 caracteres
        /// * **idRol**: Debe ser un ID de rol válido
        /// 
        /// **Posibles errores:**
        /// * 400: Datos inválidos o usuario ya existe
        /// * 401: No autenticado
        /// * 403: No es administrador
        /// </remarks>
        /// <param name="dto">Datos del nuevo usuario</param>
        /// <returns>Mensaje de confirmación</returns>
        /// <response code="200"> Usuario creado correctamente</response>
        /// <response code="400"> Error de validación</response>
        /// <response code="401"> No autenticado</response>
        /// <response code="403"> No autorizado</response>
        [Authorize(Roles = "administrador")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CrearUsuario(CrearUsuarioDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existe = await _context.Usuarios
                    .AnyAsync(u => u.Nombre == dto.Nombre);

                if (existe)
                    return BadRequest("El usuario ya existe");

                // Validar que el rol exista
                var rolExiste = await _context.Roles
                    .AnyAsync(r => r.IdRol == dto.IdRol);

                if (!rolExiste)
                    return BadRequest("El rol no existe");

                string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var usuario = new Usuario
                {
                    Nombre = dto.Nombre,
                    PasswordHash = hash,
                    IdRol = dto.IdRol,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return Ok("Usuario creado correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// INICIAR SESIÓN (LOGIN)
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Autentica un usuario y genera un token JWT para acceso al sistema.
        /// 
        /// ** Proceso de autenticación:**
        /// 1. Verifica que el usuario existe
        /// 2. Verifica que el usuario está activo
        /// 3. Compara la contraseña con BCrypt
        /// 4. Genera token JWT con claims del usuario
        /// 
        /// **Ejemplo de petición:**
        ///  
        ///     POST /api/usuarios/login
        ///     {
        ///         "nombre": "admin",
        ///         "password": "admin123"
        ///     }
        /// 
        /// ** Respuesta exitosa (token incluido):**
        /// 
        ///     {
        ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///         "expiracion": "2026-02-19T16:30:00Z"
        ///     }
        /// 
        /// **Cómo usar el token:**
        /// En todos los endpoints protegidos, agregar en el header:
        /// ```
        /// Authorization: Bearer tu-token-jwt-aqui
        /// ```
        /// 
        /// **Posibles errores:**
        /// * 401: Credenciales inválidas (usuario no existe, inactivo o contraseña incorrecta)
        /// * 400: Datos de entrada inválidos
        /// </remarks>
        /// <param name="dto">Credenciales de acceso (usuario y contraseña)</param>
        /// <returns>Token JWT y fecha de expiración</returns>
        /// <response code="200"> Login exitoso - Devuelve token</response>
        /// <response code="400"> Datos inválidos</response>
        /// <response code="401"> Credenciales incorrectas</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _context.Usuarios
                   .Include(u => u.Rol)
                   .FirstOrDefaultAsync(u => u.Nombre == dto.Nombre);

                if (usuario == null)
                    return Unauthorized("Usuario no existe");

                if (!usuario.Activo)
                    return Unauthorized("Usuario inactivo");

                bool passwordValido = BCrypt.Net.BCrypt
                    .Verify(dto.Password, usuario.PasswordHash);

                if (!passwordValido)
                    return Unauthorized("Contraseña incorrecta");

                // Obtener configuración JWT
                var jwtSettings = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetSection("Jwt");

                // Crear claims (datos dentro del token)
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.Nombre),
                    new Claim("idUsuario", usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Role, usuario.Rol.Nombre)
                };

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(
                        Convert.ToDouble(jwtSettings["DurationInMinutes"])),
                    signingCredentials: creds
                );

                var tokenGenerado = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    token = tokenGenerado,
                    expiracion = token.ValidTo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el login", error = ex.Message });
            }
        }
        /// <summary>
        ///  OBTENER PERFIL DEL USUARIO AUTENTICADO
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Devuelve la información del usuario actualmente autenticado usando el token JWT.
        ///
        /// ** Requiere autenticación:**
        /// * Token JWT válido
        /// * No importa el rol (Administrador, servicio, etc.)
        ///
        /// **¿Cómo funciona?**
        /// - El backend extrae el `idUsuario` desde el token
        /// - Busca al usuario en la base de datos
        /// - Devuelve únicamente su propia información
        ///
        /// **¿Para qué sirve?**
        /// - Mostrar perfil del usuario en la app
        /// - Evitar exponer datos de otros usuarios
        /// - Base para editar perfil o cambiar contraseña
        ///
        /// **Ejemplo de request:**
        /// 
        ///     GET /api/usuarios/me
        ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
        ///
        /// **Ejemplo de respuesta exitosa:**
        /// 
        ///     {
        ///       "idUsuario": 2,
        ///       "nombre": "servicio",
        ///       "idRol": 2,
        ///       "activo": true
        ///     }
        ///
        /// **Posibles errores:**
        /// * 401: No autenticado (token inválido o ausente)
        /// * 404: Usuario no encontrado
        /// * 500: Error interno del servidor
        /// </remarks>
        /// <returns>Datos del usuario autenticado</returns>
        /// <response code="200"> Éxito - Perfil del usuario</response>
        /// <response code="401"> No autenticado - Token requerido</response>
        /// <response code="404"> Usuario no encontrado</response>
        /// <response code="500"> Error interno</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMiPerfil()
        {
            var idUsuarioClaim = User.FindFirst("idUsuario")?.Value;

            if (idUsuarioClaim == null)
                return Unauthorized("Token inválido");

            int idUsuario = int.Parse(idUsuarioClaim);

            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new UsuarioResponseDto
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    IdRol = u.IdRol,
                    Activo = u.Activo
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            return Ok(usuario);
        }
        /// <summary>
        ///  ACTUALIZAR PERFIL DEL USUARIO AUTENTICADO
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Permite al usuario autenticado actualizar su nombre.
        ///
        /// ** Requiere autenticación:**
        /// * Token JWT válido
        /// * Disponible para cualquier usuario autenticado
        ///
        /// **¿Cómo funciona?**
        /// - Obtiene el `idUsuario` desde el token JWT
        /// - Verifica que el usuario exista
        /// - Valida que el nuevo nombre no esté duplicado
        /// - Actualiza el nombre del usuario
        ///
        /// **¿Para qué sirve?**
        /// - Permitir que el usuario edite su perfil
        /// - Mantener actualizada la información del sistema
        ///
        /// **Ejemplo de request:**
        /// 
        ///     PUT /api/usuarios/me
        ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
        ///     
        ///     {
        ///       "nombre": "nuevoNombre"
        ///     }
        ///
        /// **Ejemplo de respuesta exitosa:**
        /// 
        ///     "Perfil actualizado correctamente"
        ///
        /// **Posibles errores:**
        /// * 400: Nombre duplicado o datos inválidos
        /// * 401: No autenticado (token inválido o ausente)
        /// * 404: Usuario no encontrado
        /// * 500: Error interno del servidor
        /// </remarks>
        /// <param name="dto">Datos del perfil a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200"> Éxito - Perfil actualizado</response>
        /// <response code="400"> Datos inválidos o nombre duplicado</response>
        /// <response code="401"> No autenticado</response>
        /// <response code="404"> Usuario no encontrado</response>
        /// <response code="500"> Error interno</response>
        [Authorize]
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActualizarPerfil(ActualizarPerfilDto dto)
        {
            var idUsuarioClaim = User.FindFirst("idUsuario")?.Value;

            if (idUsuarioClaim == null)
                return Unauthorized("Token inválido");

            int idUsuario = int.Parse(idUsuarioClaim);

            var usuario = await _context.Usuarios.FindAsync(idUsuario);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            var existe = await _context.Usuarios
                .AnyAsync(u => u.Nombre == dto.Nombre && u.IdUsuario != idUsuario);

            if (existe)
                return BadRequest("El nombre ya está en uso");

            usuario.Nombre = dto.Nombre;

            await _context.SaveChangesAsync();

            return Ok("Perfil actualizado correctamente");
        }

        /// <summary>
        ///  CAMBIAR CONTRASEÑA DEL USUARIO AUTENTICADO
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Permite al usuario autenticado cambiar su contraseña de forma segura.
        ///
        /// ** Requiere autenticación:**
        /// * Token JWT válido
        /// * Disponible para cualquier usuario autenticado
        ///
        /// **¿Cómo funciona?**
        /// - Obtiene el `idUsuario` desde el token JWT
        /// - Verifica que el usuario exista
        /// - Valida que la contraseña actual sea correcta
        /// - Genera un nuevo hash seguro con BCrypt
        /// - Actualiza la contraseña en la base de datos
        ///
        /// **¿Para qué sirve?**
        /// - Permitir al usuario cambiar su contraseña
        /// - Mantener la seguridad del sistema
        ///
        /// **Ejemplo de request:**
        /// 
        ///     PUT /api/usuarios/cambiar-password
        ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
        ///     
        ///     {
        ///       "passwordActual": "123456",
        ///       "passwordNueva": "NuevaPassword123"
        ///     }
        ///
        /// **Ejemplo de respuesta exitosa:**
        /// 
        ///     "Contraseña actualizada correctamente"
        ///
        /// **Posibles errores:**
        /// * 400: Contraseña actual incorrecta
        /// * 401: No autenticado (token inválido o ausente)
        /// * 404: Usuario no encontrado
        /// * 500: Error interno del servidor
        /// </remarks>
        /// <param name="dto">Datos para cambiar contraseña</param>
        /// <returns>Resultado de la operación</returns>
        /// <response code="200"> Éxito - Contraseña actualizada</response>
        /// <response code="400"> Contraseña incorrecta</response>
        /// <response code="401"> No autenticado</response>
        /// <response code="404"> Usuario no encontrado</response>
        /// <response code="500"> Error interno</response>
        [Authorize]
        [HttpPut("cambiar-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
        {
            var idUsuarioClaim = User.FindFirst("idUsuario")?.Value;

            if (idUsuarioClaim == null)
                return Unauthorized("Token inválido");

            int idUsuario = int.Parse(idUsuarioClaim);

            var usuario = await _context.Usuarios.FindAsync(idUsuario);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            bool passwordValido = BCrypt.Net.BCrypt
                .Verify(dto.PasswordActual, usuario.PasswordHash);

            if (!passwordValido)
                return BadRequest("Contraseña actual incorrecta");

            usuario.PasswordHash = BCrypt.Net.BCrypt
                .HashPassword(dto.PasswordNueva);

            await _context.SaveChangesAsync();

            return Ok("Contraseña actualizada correctamente");
        }
        /// <summary>
        ///  RESETEAR CONTRASEÑA DE USUARIO (ADMIN)
        /// </summary>
        /// <remarks>
        /// **¿Qué hace este endpoint?**
        /// Permite al administrador resetear la contraseña de cualquier usuario.
        ///
        /// ** Requiere autenticación:**
        /// * Rol requerido: **Administrador**
        ///
        /// **¿Para qué sirve?**
        /// - Recuperación de cuentas
        /// - Soporte técnico
        ///
        /// **Ejemplo:**
        /// 
        ///     PUT /api/usuarios/reset-password/2
        ///     
        ///     {
        ///       "nuevaPassword": "123456"
        ///     }
        ///
        /// **Errores:**
        /// * 401: No autenticado
        /// * 403: No autorizado
        /// * 404: Usuario no encontrado
        /// </remarks>
        [Authorize(Roles = "administrador")]
        [HttpPut("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound("Usuario no encontrado");

            usuario.PasswordHash = BCrypt.Net.BCrypt
                .HashPassword(dto.NuevaPassword);

            await _context.SaveChangesAsync();

            return Ok("Contraseña reseteada correctamente");
        }

    }
}
