using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class AddNotaCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotaCredito",
                columns: table => new
                {
                    IdNotaCredito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNroVenta = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotaCredito", x => x.IdNotaCredito);
                    table.ForeignKey(
                        name: "FK_NotaCredito_venta_IdNroVenta",
                        column: x => x.IdNroVenta,
                        principalTable: "venta",
                        principalColumn: "id_nro_venta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleNotaCredito",
                columns: table => new
                {
                    IdDetalleNotaCredito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdNotaCredito = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleNotaCredito", x => x.IdDetalleNotaCredito);
                    table.ForeignKey(
                        name: "FK_DetalleNotaCredito_NotaCredito_IdNotaCredito",
                        column: x => x.IdNotaCredito,
                        principalTable: "NotaCredito",
                        principalColumn: "IdNotaCredito",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleNotaCredito_producto_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "producto",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleNotaCredito_IdNotaCredito",
                table: "DetalleNotaCredito",
                column: "IdNotaCredito");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleNotaCredito_IdProducto",
                table: "DetalleNotaCredito",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_NotaCredito_IdNroVenta",
                table: "NotaCredito",
                column: "IdNroVenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleNotaCredito");

            migrationBuilder.DropTable(
                name: "NotaCredito");
        }
    }
}
