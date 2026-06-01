using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations; 

namespace restauranteBD.Models
{
    /// <summary>
    /// Modelo que representa la tabla 'roles' en la base de datos
    /// </summary>
    /// <remarks>
    /// **Tabla:** roles (PostgreSQL)
    /// **Esquema:** public
    /// 
    /// Este modelo mapea directamente la estructura de la tabla roles
    /// y sus relaciones con otras tablas del sistema.
    /// 
    /// **Relaciones:**
    /// * Un rol puede tener muchos usuarios (1 a N)
    /// 
    /// **Datos almacenados:**
    /// - ID único del rol (autogenerado)
    /// - Nombre del rol (único)
    /// - Descripción del rol
    /// </remarks>
    [Table("roles", Schema = "public")]
    public class Rol
    {
        /// <summary>
        /// Identificador único del rol (Primary Key)
        /// </summary>
        /// <remarks>
        /// * Corresponde a la columna 'id_rol' en la base de datos
        /// * Se genera automáticamente (autoincremental)
        /// * Es único para cada registro
        /// * Se usa como llave foránea en la tabla Usuarios
        /// </remarks>
        /// <example>1</example>
        [Key]
        [Column("id_rol")]
        public int IdRol { get; set; }

        /// <summary>
        /// Nombre del rol
        /// </summary>
        /// <remarks>
        /// * Corresponde a la columna 'nombre' en la base de datos
        /// * Tipo: varchar(50) o similar
        /// * Debe ser único (no pueden existir dos roles con el mismo nombre)
        /// * Campo obligatorio (NOT NULL)
        /// * Se usa para identificar el rol en el sistema
        /// 
        /// **Valores típicos:**
        /// - "Administrador"
        /// - "Mesero" 
        /// - "Cajero"
        /// - "Cocinero"
        /// - "Gerente"
        /// - "Cliente"
        /// </remarks>
        /// <example>Administrador</example>
        [Column("nombre")]
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        /// <remarks>
        /// * Corresponde a la columna 'descripcion' en la base de datos
        /// * Tipo: varchar(200) o text
        /// * Campo opcional (puede ser NULL)
        /// * Explica las funciones y permisos del rol
        /// 
        /// **Ejemplos de descripciones:**
        /// - "Acceso total al sistema, puede gestionar usuarios y configuraciones"
        /// - "Puede tomar pedidos, gestionar mesas y ver el menú"
        /// - "Puede procesar pagos, generar facturas y cerrar caja"
        /// - "Puede ver pedidos pendientes y marcar como listos"
        /// </remarks>
        /// <example>Acceso total al sistema, puede gestionar usuarios y configuraciones</example>
        [Column("descripcion")]
        public string Descripcion { get; set; }

        /// <summary>
        /// Colección de usuarios que tienen este rol
        /// </summary>
        /// <remarks>
        /// **Propiedad de navegación (Navigation Property)**
        /// 
        /// * Relación: Uno a Muchos (1 rol → N usuarios)
        /// * Un rol puede estar asignado a múltiples usuarios
        /// * Esta propiedad permite acceder a los usuarios relacionados
        /// * No se mapea directamente a una columna en la tabla
        /// * Se usa para consultas con Include() en Entity Framework
        /// 
        /// **Ejemplo de uso en LINQ:**
        /// ```csharp
        /// var rolConUsuarios = await _context.Roles
        ///     .Include(r => r.Usuarios)
        ///     .FirstOrDefaultAsync(r => r.IdRol == 1);
        /// ```
        /// </remarks>
        public ICollection<Usuario> Usuarios { get; set; }
    }
}