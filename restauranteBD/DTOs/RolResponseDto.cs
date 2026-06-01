using Swashbuckle.AspNetCore.Annotations; 

namespace restauranteBD.DTOs
{
    /// <summary>
    /// DTO (Data Transfer Object) para la respuesta de roles
    /// </summary>
    /// <remarks>
    /// Este objeto se utiliza para enviar la información de los roles 
    /// desde el backend hacia el frontend.
    /// 
    /// **¿Por qué usar un DTO?**
    /// - Oculta campos sensibles de la base de datos
    /// - Solo envía la información necesaria
    /// - Mejor rendimiento en la red
    /// </remarks>
    public class RolResponseDto
    {
        /// <summary>
        /// Identificador único del rol
        /// </summary>
        /// <remarks>
        /// * Corresponde al ID en la tabla Roles de la base de datos
        /// * Se genera automáticamente al crear el rol
        /// * Es único para cada rol
        /// </remarks>
        /// <example>1</example>
        public int IdRol { get; set; }

        /// <summary>
        /// Nombre del rol
        /// </summary>
        /// <remarks>
        /// * Debe ser único en el sistema
        /// * Longitud máxima: 50 caracteres
        /// * No puede estar vacío
        /// 
        /// **Ejemplos comunes:**
        /// - Administrador
        /// - Mesero
        /// - Cajero
        /// - Cocinero
        /// - Gerente
        /// </remarks>
        /// <example>Administrador</example>
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción detallada del rol
        /// </summary>
        /// <remarks>
        /// * Explica las funciones y permisos del rol
        /// * Longitud máxima: 200 caracteres
        /// * Puede estar vacío (opcional)
        /// 
        /// **Ejemplos:**
        /// - "Acceso total al sistema, puede gestionar usuarios"
        /// - "Puede tomar pedidos y gestionar mesas"
        /// - "Puede procesar pagos y generar facturas"
        /// </remarks>
        /// <example>Acceso total al sistema, puede gestionar usuarios y configuraciones</example>
        public string Descripcion { get; set; }
    }
}