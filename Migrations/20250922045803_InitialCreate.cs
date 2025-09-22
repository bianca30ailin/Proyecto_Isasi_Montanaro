using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__categori__CD54BC5A610BB531", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    dni_cliente = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cliente__F53D4BA5B5774DF0", x => x.dni_cliente);
                });

            migrationBuilder.CreateTable(
                name: "estado",
                columns: table => new
                {
                    id_estado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__estado__86989FB20C055FB1", x => x.id_estado);
                });

            migrationBuilder.CreateTable(
                name: "provincia",
                columns: table => new
                {
                    id_provincia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__provinci__66C18BFDA3FE9356", x => x.id_provincia);
                });

            migrationBuilder.CreateTable(
                name: "tipo_usuario",
                columns: table => new
                {
                    id_tipo_usuario = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tipo_usu__B17D78C8165631FD", x => x.id_tipo_usuario);
                });

            migrationBuilder.CreateTable(
                name: "transporte",
                columns: table => new
                {
                    id_transporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__transpor__7AC9B3AEB3C4D8CC", x => x.id_transporte);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    apellido = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    dni = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Telefono = table.Column<int>(type: "int", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contraseña = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    baja = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__usuario__4E3E04ADF6B4BB0C", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "producto",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    precio = table.Column<double>(type: "float", nullable: false),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    stock_minimo = table.Column<int>(type: "int", nullable: false),
                    baja = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__producto__FF341C0D810F0BF8", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK__producto__id_cat__6B24EA82",
                        column: x => x.id_categoria,
                        principalTable: "categoria",
                        principalColumn: "id_categoria");
                });

            migrationBuilder.CreateTable(
                name: "ciudad",
                columns: table => new
                {
                    id_ciudad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    id_provincia = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ciudad__B7DC4CD5C121E7BB", x => x.id_ciudad);
                    table.ForeignKey(
                        name: "FK__ciudad__id_provi__4BAC3F29",
                        column: x => x.id_provincia,
                        principalTable: "provincia",
                        principalColumn: "id_provincia");
                });

            migrationBuilder.CreateTable(
                name: "funcion_usuario",
                columns: table => new
                {
                    id_tipo_usuario = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__funcion___759E98823F5F8A42", x => new { x.id_tipo_usuario, x.id_usuario });
                    table.ForeignKey(
                        name: "FK__funcion_u__id_ti__6E01572D",
                        column: x => x.id_tipo_usuario,
                        principalTable: "tipo_usuario",
                        principalColumn: "id_tipo_usuario");
                    table.ForeignKey(
                        name: "FK__funcion_u__id_us__6EF57B66",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "venta",
                columns: table => new
                {
                    id_nro_venta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaHora = table.Column<DateOnly>(type: "date", nullable: false),
                    total = table.Column<double>(type: "float", nullable: false),
                    dni_cliente = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__venta__D47121A5607A29DB", x => x.id_nro_venta);
                    table.ForeignKey(
                        name: "FK__venta__dni_clien__628FA481",
                        column: x => x.dni_cliente,
                        principalTable: "cliente",
                        principalColumn: "dni_cliente");
                    table.ForeignKey(
                        name: "FK__venta__id_usuari__6383C8BA",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateTable(
                name: "direccion",
                columns: table => new
                {
                    id_direccion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombreCalle = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    altura = table.Column<int>(type: "int", nullable: false),
                    codigo_postal = table.Column<int>(type: "int", nullable: false),
                    id_ciudad = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__direccio__25C35D07EA902764", x => x.id_direccion);
                    table.ForeignKey(
                        name: "FK__direccion__id_ci__4E88ABD4",
                        column: x => x.id_ciudad,
                        principalTable: "ciudad",
                        principalColumn: "id_ciudad");
                });

            migrationBuilder.CreateTable(
                name: "detalle_venta_producto",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidad = table.Column<int>(type: "int", nullable: false),
                    subtotal = table.Column<double>(type: "float", nullable: false),
                    id_nro_venta = table.Column<int>(type: "int", nullable: false),
                    id_producto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__detalle___4F1332DE0C4A86F4", x => x.id_detalle);
                    table.ForeignKey(
                        name: "FK__detalle_v__id_nr__71D1E811",
                        column: x => x.id_nro_venta,
                        principalTable: "venta",
                        principalColumn: "id_nro_venta");
                    table.ForeignKey(
                        name: "FK__detalle_v__id_pr__72C60C4A",
                        column: x => x.id_producto,
                        principalTable: "producto",
                        principalColumn: "id_producto");
                });

            migrationBuilder.CreateTable(
                name: "envio",
                columns: table => new
                {
                    id_envio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fehca_despacho = table.Column<DateOnly>(type: "date", nullable: false),
                    num_seguimiento = table.Column<string>(type: "varchar(25)", unicode: false, maxLength: 25, nullable: false),
                    costo = table.Column<double>(type: "float", nullable: false),
                    id_estado = table.Column<int>(type: "int", nullable: false),
                    id_transporte = table.Column<int>(type: "int", nullable: false),
                    id_nro_venta = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__envio__8C48C8CAEABDBD3A", x => x.id_envio);
                    table.ForeignKey(
                        name: "FK__envio__id_estado__66603565",
                        column: x => x.id_estado,
                        principalTable: "estado",
                        principalColumn: "id_estado");
                    table.ForeignKey(
                        name: "FK__envio__id_nro_ve__68487DD7",
                        column: x => x.id_nro_venta,
                        principalTable: "venta",
                        principalColumn: "id_nro_venta");
                    table.ForeignKey(
                        name: "FK__envio__id_transp__6754599E",
                        column: x => x.id_transporte,
                        principalTable: "transporte",
                        principalColumn: "id_transporte");
                });

            migrationBuilder.CreateTable(
                name: "direccion_cliente",
                columns: table => new
                {
                    id_direccion = table.Column<int>(type: "int", nullable: false),
                    dni_cliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__direccio__6A9089BDB397B0E7", x => new { x.id_direccion, x.dni_cliente });
                    table.ForeignKey(
                        name: "FK__direccion__dni_c__5441852A",
                        column: x => x.dni_cliente,
                        principalTable: "cliente",
                        principalColumn: "dni_cliente");
                    table.ForeignKey(
                        name: "FK__direccion__id_di__534D60F1",
                        column: x => x.id_direccion,
                        principalTable: "direccion",
                        principalColumn: "id_direccion");
                });

            migrationBuilder.InsertData(
                table: "tipo_usuario",
                columns: new[] { "id_tipo_usuario", "descripcion", "tipo" },
                values: new object[,]
                {
                    { 1, "Usuario con acceso total al sistema", "Admin" },
                    { 2, "Usuario que registra las ventas", "Ventas" },
                    { 3, "Usuario que se encarga de registrar los envíos y sus estados", "Logistica" },
                    { 4, "Usuario que se encarga del inventario", "Inventario" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ciudad_id_provincia",
                table: "ciudad",
                column: "id_provincia");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_producto_id_nro_venta",
                table: "detalle_venta_producto",
                column: "id_nro_venta");

            migrationBuilder.CreateIndex(
                name: "IX_detalle_venta_producto_id_producto",
                table: "detalle_venta_producto",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_direccion_id_ciudad",
                table: "direccion",
                column: "id_ciudad");

            migrationBuilder.CreateIndex(
                name: "IX_direccion_cliente_dni_cliente",
                table: "direccion_cliente",
                column: "dni_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_envio_id_estado",
                table: "envio",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_envio_id_nro_venta",
                table: "envio",
                column: "id_nro_venta");

            migrationBuilder.CreateIndex(
                name: "IX_envio_id_transporte",
                table: "envio",
                column: "id_transporte");

            migrationBuilder.CreateIndex(
                name: "IX_funcion_usuario_id_usuario",
                table: "funcion_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_producto_id_categoria",
                table: "producto",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "UQ__usuario__AB6E61642F811227",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__usuario__D87608A7AF6E4AC8",
                table: "usuario",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venta_dni_cliente",
                table: "venta",
                column: "dni_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_venta_id_usuario",
                table: "venta",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalle_venta_producto");

            migrationBuilder.DropTable(
                name: "direccion_cliente");

            migrationBuilder.DropTable(
                name: "envio");

            migrationBuilder.DropTable(
                name: "funcion_usuario");

            migrationBuilder.DropTable(
                name: "producto");

            migrationBuilder.DropTable(
                name: "direccion");

            migrationBuilder.DropTable(
                name: "estado");

            migrationBuilder.DropTable(
                name: "venta");

            migrationBuilder.DropTable(
                name: "transporte");

            migrationBuilder.DropTable(
                name: "tipo_usuario");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "ciudad");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "provincia");
        }
    }
}
