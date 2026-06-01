using Swashbuckle.AspNetCore.Annotations; 

namespace restauranteBD.DTOs
{
    /// <summary>
    /// DTO para la respuesta de datos de usuarios
    /// </summary>
    /// <remarks>
    /// Este DTO se utiliza para enviar información de usuarios al cliente (frontend).
    /// Contiene solo los datos necesarios para mostrar en la interfaz.
    /// 
    /// **¿Qué información incluye?**
    /// - Identificador único del usuario
    /// - Nombre de usuario
    /// - Rol asignado (solo ID)
    /// - Estado del usuario (activo/inactivo)
    /// 
    /// **Información que NO incluye (seguridad):**
    /// -  Password (nunca se envía)
    /// -  PasswordHash
    /// -  FechaCreación (si no es necesaria)
    /// -  Datos sensibles
    /// 
    /// **¿Para qué se usa?**
    /// - Listados de usuarios (GET /api/usuarios)
    /// - Detalle de usuario
    /// - Respuestas después de crear/modificar
    /// </remarks>
    public class UsuarioResponseDto
    {
        /// <summary>
        /// Identificador único del usuario
        /// </summary>
        /// <remarks>
        /// * Corresponde al ID en la tabla Usuarios
        /// * Se genera automáticamente en la base de datos
        /// * Único para cada usuario
        /// * Se usa para referenciar al usuario en otras operaciones
        /// </remarks>
        /// <example>1</example>
        public int IdUsuario { get; set; }

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        /// <remarks>
        /// * Nombre único de acceso al sistema
        /// * Se usa para iniciar sesión
        /// * Visible en la interfaz
        /// * No se puede modificar fácilmente (depende de la lógica)
        /// 
        /// **Ejemplos:**
        /// - admin
        /// - juan.perez
        /// - maria.2024
        /// - cocinero.principal
        /// </remarks>
        /// <example>admin</example>
        public string Nombre { get; set; }

        /// <summary>
        /// ID del rol asignado al usuario
        /// </summary>
        /// <remarks>
        /// * Referencia a la tabla Roles
        /// * Define los permisos del usuario
        /// * Se usa para autorización ([Authorize(Roles = "...")])
        /// 
        /// **Valores posibles:**
        /// 
        /// | ID | Rol | Descripción |
        /// |----|-----|-------------|
        /// | 1 | administrador | Acceso total |
        /// | 2 | servicio | Comandas y mesas |

        /// 
        /// Para obtener el nombre del rol, se debe hacer un JOIN con la tabla Roles
        /// </remarks>
        /// <example>1</example>
        public int IdRol { get; set; }

        /// <summary>
        /// Estado del usuario
        /// </summary>
        /// <remarks>
        /// * **true** = Usuario activo (puede iniciar sesión)
        /// * **false** = Usuario inactivo (no puede acceder)
        /// 
        /// **¿Para qué sirve?**
        /// - Desactivar usuarios sin eliminarlos
        /// - Bloquear acceso temporalmente
        /// - Mantener historial sin dar acceso
        /// 
        /// **Comportamiento:**
        /// - Si Activo = false, el login rechazará al usuario
        /// - Los usuarios inactivos no pueden usar el sistema
        /// - Útil para bajas laborales o despidos
        /// </remarks>
        /// <example>true</example>
        public bool Activo { get; set; }
    }
}