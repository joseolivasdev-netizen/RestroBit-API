using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace restauranteBD.Migrations
{
    /// <inheritdoc />
    public partial class SepararEstadosComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuarios");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "usuarios",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                schema: "public",
                table: "usuarios",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "Password",
                schema: "public",
                table: "usuarios",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "usuarios",
                newName: "id_rol");

            migrationBuilder.AlterColumn<int>(
                name: "id_rol",
                schema: "public",
                table: "usuarios",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario",
                schema: "public",
                table: "usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<bool>(
                name: "activo",
                schema: "public",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                schema: "public",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_usuarios",
                schema: "public",
                table: "usuarios",
                column: "id_usuario");

            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    IdCuenta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMesa = table.Column<int>(type: "integer", nullable: true),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.IdCuenta);
                });

            migrationBuilder.CreateTable(
                name: "destinos",
                columns: table => new
                {
                    id_destino = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destinos", x => x.id_destino);
                });

            migrationBuilder.CreateTable(
                name: "mesas",
                columns: table => new
                {
                    id_mesa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    capacidad = table.Column<int>(type: "integer", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesas", x => x.id_mesa);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_destino = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id_categoria);
                    table.ForeignKey(
                        name: "FK_categorias_destinos_id_destino",
                        column: x => x.id_destino,
                        principalTable: "destinos",
                        principalColumn: "id_destino");
                });

            migrationBuilder.CreateTable(
                name: "comandas",
                columns: table => new
                {
                    id_comanda = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cuenta = table.Column<int>(type: "integer", nullable: true),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_mesa = table.Column<int>(type: "integer", nullable: true),
                    fecha_apertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado_cocina = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente"),
                    estado_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "por_cobrar"),
                    nombre_cliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comandas", x => x.id_comanda);
                    table.ForeignKey(
                        name: "FK_comandas_Cuentas_id_cuenta",
                        column: x => x.id_cuenta,
                        principalTable: "Cuentas",
                        principalColumn: "IdCuenta");
                    table.ForeignKey(
                        name: "FK_comandas_mesas_id_mesa",
                        column: x => x.id_mesa,
                        principalTable: "mesas",
                        principalColumn: "id_mesa",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_comandas_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "public",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    costo_estimado = table.Column<decimal>(type: "numeric", nullable: true),
                    margen_ganancia = table.Column<decimal>(type: "numeric", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_productos_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalle_comanda",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_comanda = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    notas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado_item = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalle_comanda", x => x.id_detalle);
                    table.ForeignKey(
                        name: "FK_detalle_comanda_comandas_id_comanda",
                        column: x => x.id_comanda,
                        principalTable: "comandas",
                        principalColumn: "id_comanda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalle_comanda_productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "producto_presentaciones",
                columns: table => new
                {
                    id_presentacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    nombre_presentacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    precio = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_presentaciones", x => x.id_presentacion);
                    table.ForeignKey(
                        name: "FK_producto_presentaciones_productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "mesas",
                columns: new[] { "id_mesa", "activa", "capacidad", "nombre", "tipo" },
                values: new object[,]
                {
                    { 1, true, 4, "M1", "interior" },
                    { 2, true, 4, "M2", "interior" },
                    { 3, true, 6, "M3", "terraza" },
                    { 4, true, 2, "M4", "terraza" },
                    { 5, true, 8, "Barra", "barra" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_rol",
                schema: "public",
                table: "usuarios",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_id_destino",
                table: "categorias",
                column: "id_destino");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_estado_cocina",
                table: "comandas",
                column: "estado_cocina");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_estado_pago",
                table: "comandas",
                column: "estado_pago");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_id_cuenta",
                table: "comandas",
                column: "id_cuenta");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_id_mesa",
                table: "comandas",
                column: "id_mesa",
                filter: "estado_pago != 'pagada'");

            migrationBuilder.CreateIndex(
                name: "IX_comandas_id_usuario",
                table: "comandas",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_comanda_estado_item",
                table: "detalle_comanda",
                column: "estado_item");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_comanda_id_comanda",
                table: "detalle_comanda",
                column: "id_comanda");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_comanda_id_producto",
                table: "detalle_comanda",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_mesas_activa",
                table: "mesas",
                column: "activa");

            migrationBuilder.CreateIndex(
                name: "IX_producto_presentaciones_id_producto",
                table: "producto_presentaciones",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_productos_id_categoria",
                table: "productos",
                column: "id_categoria");

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_roles_id_rol",
                schema: "public",
                table: "usuarios",
                column: "id_rol",
                principalSchema: "public",
                principalTable: "roles",
                principalColumn: "id_rol",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_roles_id_rol",
                schema: "public",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "detalle_comanda");

            migrationBuilder.DropTable(
                name: "producto_presentaciones");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "comandas");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropTable(
                name: "mesas");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "destinos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_usuarios",
                schema: "public",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_id_rol",
                schema: "public",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "id_usuario",
                schema: "public",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "activo",
                schema: "public",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                schema: "public",
                table: "usuarios");

            migrationBuilder.RenameTable(
                name: "usuarios",
                schema: "public",
                newName: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Usuarios",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Usuarios",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "id_rol",
                table: "Usuarios",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios",
                column: "Id");
        }
    }
}
