using Microsoft.EntityFrameworkCore;
using restauranteBD.Models;

namespace restauranteBD.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoPresentacion> ProductoPresentaciones { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<Comanda> Comandas { get; set; }
        public DbSet<DetalleComanda> DetalleComandas { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Destino> Destinos { get; set; }
        public DbSet<CorteCaja> CortesCaja { get; set; }
        public DbSet<Egreso> Egresos { get; set; } // <--- AGRÉGALA AQUÍ



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de MESA
            modelBuilder.Entity<Mesa>(entity =>
            {
                entity.HasKey(m => m.IdMesa);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Tipo).HasMaxLength(20);
                entity.Property(m => m.Capacidad);
                entity.Property(m => m.Activa).HasDefaultValue(true);

                // Índice para búsquedas rápidas
                entity.HasIndex(m => m.Activa);
            });

            // Configuración de COMANDA
            modelBuilder.Entity<Comanda>(entity =>
            {
                entity.HasKey(c => c.IdComanda);

                // 👇 CORRECCIÓN: Configuramos las dos columnas nuevas
                entity.Property(c => c.EstadoCocina).IsRequired().HasMaxLength(20).HasDefaultValue("pendiente");
                entity.Property(c => c.EstadoPago).IsRequired().HasMaxLength(20).HasDefaultValue("por_cobrar");

                entity.Property(c => c.FechaApertura).IsRequired();
                entity.Property(c => c.FechaCierre);

                // Relación con Mesa (ahora nullable)
                entity.HasOne(c => c.Mesa)
                    .WithMany(m => m.Comandas)
                    .HasForeignKey(c => c.IdMesa)
                    .OnDelete(DeleteBehavior.SetNull); // Si se borra mesa, comanda queda sin mesa

                // Relación con Usuario
                entity.HasOne(c => c.Usuario)
                    .WithMany(u => u.Comandas)
                    .HasForeignKey(c => c.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict); // No permitir borrar usuario con comandas

                // 👇 CORRECCIÓN: Índices actualizados
                entity.HasIndex(c => c.EstadoCocina);
                entity.HasIndex(c => c.EstadoPago);
                // Cambiamos el filtro del índice para buscar comandas que no estén pagadas
                entity.HasIndex(c => c.IdMesa).HasFilter("estado_pago != 'pagada'");
            });

            // Configuración de DETALLE COMANDA
            modelBuilder.Entity<DetalleComanda>(entity =>
            {
                entity.HasKey(d => d.IdDetalle);
                entity.Property(d => d.Cantidad).IsRequired();
                entity.Property(d => d.PrecioUnitario).IsRequired().HasPrecision(10, 2);
                entity.Property(d => d.Notas).HasMaxLength(200);
                entity.Property(d => d.EstadoItem).HasMaxLength(20).HasDefaultValue("pendiente");

                // Relación con Comanda
                entity.HasOne(d => d.Comanda)
                    .WithMany(c => c.Detalles)
                    .HasForeignKey(d => d.IdComanda)
                    .OnDelete(DeleteBehavior.Cascade); // Si borras comanda, borras detalles

                // Relación con Producto
                entity.HasOne(d => d.Presentacion)
       .WithMany()
       .HasForeignKey(d => d.IdPresentacion)
       .OnDelete(DeleteBehavior.Restrict);

                // Índices
                entity.HasIndex(d => d.IdComanda);
                entity.HasIndex(d => d.EstadoItem);
            });

            // Configuración de PRODUCTO (asumiendo que existe)
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.IdProducto);
                entity.HasMany(p => p.Presentaciones)
           .WithOne(pr => pr.Producto)
           .HasForeignKey(pr => pr.IdProducto);
            });

            // Configuración de EGRESO
            modelBuilder.Entity<Egreso>(entity =>
            {
                entity.HasKey(e => e.IdEgreso);

                entity.Property(e => e.Monto)
                    .IsRequired()
                    .HasPrecision(18, 2); // Precisión monetaria estándar

                entity.Property(e => e.Concepto)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Fecha)
                    .IsRequired();

                // Relación con el Corte de Caja (opcional pero recomendada)
                entity.HasOne<CorteCaja>()
                    .WithMany()
                    .HasForeignKey(e => e.IdCorte)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Datos semilla para mesas (opcional)
            modelBuilder.Entity<Mesa>().HasData(
                new Mesa { IdMesa = 1, Nombre = "M1", Tipo = "interior", Capacidad = 4, Activa = true },
                new Mesa { IdMesa = 2, Nombre = "M2", Tipo = "interior", Capacidad = 4, Activa = true },
                new Mesa { IdMesa = 3, Nombre = "M3", Tipo = "terraza", Capacidad = 6, Activa = true },
                new Mesa { IdMesa = 4, Nombre = "M4", Tipo = "terraza", Capacidad = 2, Activa = true },
                new Mesa { IdMesa = 5, Nombre = "Barra", Tipo = "barra", Capacidad = 8, Activa = true }
            );
        }
    }
}