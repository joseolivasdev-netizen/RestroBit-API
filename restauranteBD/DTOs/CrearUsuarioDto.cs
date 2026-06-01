using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations; 

namespace restauranteBD.DTOs
{
    /// <summary>
    /// DTO para la creación de nuevos usuarios en el sistema
    /// </summary>
    /// <remarks>
    /// Este DTO se utiliza cuando un administrador quiere crear un nuevo usuario.
    /// Todos los campos son obligatorios y deben cumplir las validaciones.
    /// 
    /// **Proceso de creación:**
    /// 1. El administrador envía estos datos
    /// 2. Se valida que el nombre no exista
    /// 3. Se valida que el rol exista
    /// 4. La contraseña se encripta con BCrypt
    /// 5. El usuario se guarda con estado Activo = true
    /// </remarks>
    public class CrearUsuarioDto
    {
        /// <summary>
        /// Nombre de usuario único
        /// </summary>
        /// <remarks>
        /// * **Requerido:** Sí
        /// * **Longitud mínima:** 3 caracteres
        /// * **Longitud máxima:** 50 caracteres
        /// * **Debe ser único** en el sistema
        /// * No puede contener espacios
        /// * Solo letras y números
        /// 
        /// **Ejemplos válidos:**
        /// - admin
        /// - juan.perez
        /// - cocinero1
        /// 
        /// **Ejemplos inválidos:**
        /// - a (muy corto)
        /// - admin admin (con espacios)
        /// - usuario@existe (si ya existe)
        /// </remarks>
        /// <example>juan.perez</example>
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9.]+$", ErrorMessage = "Solo letras, números y puntos")]
        public string Nombre { get; set; }

        /// <summary>
        /// Contraseña del usuario
        /// </summary>
        /// <remarks>
        /// * **Requerido:** Sí
        /// * **Longitud mínima:** 6 caracteres
        /// * **Longitud máxima:** 100 caracteres
        /// * Se almacenará encriptada (BCrypt hash)
        /// * No se guarda en texto plano
        /// * No se puede recuperar, solo resetear
        /// 
        /// **Requisitos de seguridad:**
        /// - Mínimo 6 caracteres
        /// - Al menos 1 letra mayúscula (recomendado)
        /// - Al menos 1 número (recomendado)
        /// - Al menos 1 carácter especial (recomendado)
        /// 
        /// **Ejemplos de contraseñas seguras:**
        /// - Admin123!
        /// - C0c1n3r0@2024
        /// - Mesero.2025#
        /// 
        /// **Ejemplo de contraseña débil (evitar):**
        /// - 123456
        /// - password
        /// - admin
        /// </remarks>
        /// <example>Admin123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string Password { get; set; }

        /// <summary>
        /// ID del rol asignado al usuario
        /// </summary>
        /// <remarks>
        /// * **Requerido:** Sí
        /// * **Debe existir** en la tabla Roles
        /// * Define los permisos del usuario
        /// 
        /// **Roles disponibles en el sistema:**
        /// 
        /// | ID | Nombre | Descripción |
        /// |----|--------|-------------|
        /// | 1 | administrador | Acceso total al sistema |
        /// | 2 | servicio | Acceso a comandas y mesas |
       
        /// 
        /// **Ejemplos de uso:**
        /// - Si quieres crear un mesero → IdRol = 2
        /// - 
        /// - Si quieres crear un administrador → IdRol = 1
        /// </remarks>
        /// <example>2</example>
        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del rol debe ser válido")]
        public int IdRol { get; set; }
    }
}