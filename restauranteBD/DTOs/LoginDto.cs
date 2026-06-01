using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace restauranteBD.DTOs
{
    /// <summary>
    /// DTO para el inicio de sesión de usuarios
    /// </summary>
    /// <remarks>
    /// Este DTO se utiliza para autenticar usuarios en el sistema.
    /// Recibe las credenciales y devuelve un token JWT si son válidas.
    /// 
    /// ** Proceso de autenticación:**
    /// 1. El usuario envía sus credenciales (nombre y contraseña)
    /// 2. El sistema busca el usuario por nombre
    /// 3. Verifica que el usuario esté ACTIVO
    /// 4. Compara la contraseña con BCrypt
    /// 5. Si todo es correcto → genera token JWT
    /// 
    /// ** Seguridad:**
    /// * La contraseña viaja encriptada (HTTPS)
    /// * Nunca se devuelve la contraseña en la respuesta
    /// * El token expira después de X minutos
    /// 
    /// ** Ejemplo de petición:**
    /// 
    ///     POST /api/usuarios/login
    ///     {
    ///         "nombre": "admin",
    ///         "password": "admin123"
    ///     }
    /// 
    /// ** Ejemplo de respuesta exitosa:**
    /// 
    ///     {
    ///         "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///         "expiracion": "2026-02-19T17:30:00Z"
    ///     }
    ///
    /// ** Posibles errores:**
    /// * 400: Datos inválidos (campos vacíos)
    /// * 401: Credenciales incorrectas (usuario no existe, inactivo o password incorrecto)
    /// </remarks>
    public class LoginDto
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        /// <remarks>
        /// * **Requerido:** Sí
        /// * Corresponde al nombre único de usuario registrado
        /// * No distingue entre mayúsculas/minúsculas (depende de implementación)
        /// * Debe coincidir exactamente con el almacenado
        /// 
        /// **Ejemplos válidos:**
        /// - admin
        /// - juan.perez
        /// - cocinero1
        /// - maria_2024
        /// 
        /// **Validaciones:**
        /// - No puede estar vacío
        /// - Máximo 50 caracteres
        /// - Debe existir en la base de datos
        /// </remarks>
        /// <example>admin</example>
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string Nombre { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <remarks>
        /// * **Requerido:** Sí
        /// * Se compara con el hash almacenado usando BCrypt
        /// * No se envía en texto plano si usas HTTPS
        /// * Sensible a mayúsculas/minúsculas
        /// 
        /// **Requisitos:**
        /// - Mínimo 6 caracteres (según validación de creación)
        /// - Máximo 100 caracteres
        /// - No puede estar vacía
        /// 
        /// ** IMPORTANTE:**
        /// * Nunca incluyas la contraseña en código o repositorios
        /// * Siempre usa HTTPS en producción
        /// * La contraseña NUNCA se devuelve en la respuesta
        /// 
        /// **Ejemplos de contraseñas:**
        /// - admin123
        /// - MiPassword2024!
        /// - cocinero.123
        /// 
        /// **Nota de seguridad:**
        /// Por razones de seguridad, no se indica si el error es
        /// por usuario no existente o contraseña incorrecta (solo dice "Credenciales inválidas")
        /// </remarks>
        /// <example>admin123</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Password { get; set; }
    }
}