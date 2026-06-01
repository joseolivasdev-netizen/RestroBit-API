using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.AspNetCore.Annotations;

namespace restauranteBD.Models
{
    /// <summary>
    /// Modelo que representa la tabla 'usuarios' en la base de datos
    /// </summary>
    /// <remarks>
    /// **Tabla:** usuarios (PostgreSQL)
    /// **Esquema:** public
    /// 
    /// Este modelo almacena toda la información de los usuarios del sistema
    /// incluyendo credenciales, roles y estado.
    /// 
    /// ** Seguridad:**
    /// * Las contraseñas NUNCA se guardan en texto plano
    /// * Se almacena solo el hash (BCrypt)
    /// * Campo PasswordHash no es visible en respuestas API
    /// 
    /// **📊 Relaciones:**
    /// * **Muchos a Uno:** Muchos usuarios pueden tener un mismo rol
    ///   (Un rol → Muchos usuarios)
    /// * **Uno a Muchos:** Un usuario puede tener muchas comandas
    ///   (Un usuario → Muchas comandas)
    /// 
    /// **📋 Campos principales:**
    /// - id_usuario: Identificador único (autogenerado)
    /// - nombre: Usuario único para login
    /// - password_hash: Contraseña encriptada
    /// - id_rol: Rol asignado (FK)
    /// - activo: Estado del usuario
    /// - fecha_creacion: Cuándo se registró
    /// </remarks>
    [Table("usuarios", Schema = "public")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("activo")]
        public bool Activo { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        // Relación con Rol (existente)
        [ForeignKey("IdRol")]
        public Rol Rol { get; set; }

        // ✅ NUEVA: Relación con Comandas
        /// <summary>
        /// Colección de comandas atendidas por este usuario
        /// </summary>
        /// <remarks>
        /// **Relación:** Uno a Muchos (1:N)
        /// **Foreign Key:** IdUsuario en tabla Comandas
        /// 
        /// Un mesero puede atender múltiples comandas a lo largo del día.
        /// Esta propiedad permite navegar desde un usuario a todas
        /// las comandas que ha registrado.
        /// 
        /// **Ejemplo de uso:**
        /// 
        ///     var comandasDelDia = await _context.Usuarios
        ///         .Include(u => u.Comandas
        ///             .Where(c => c.FechaApertura.Date == hoy))
        ///         .FirstOrDefaultAsync(u => u.IdUsuario = meseroId);
        /// 
        /// **Nota:** Esta propiedad no se mapea a una columna
        /// </remarks>
        public ICollection<Comanda> Comandas { get; set; } = new List<Comanda>();
    }
}