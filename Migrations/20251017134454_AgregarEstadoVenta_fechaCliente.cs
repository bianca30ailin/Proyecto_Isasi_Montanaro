using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoVenta_fechaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoVentaId",
                table: "venta",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "cliente",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstadoVenta",
                columns: table => new
                {
                    IdEstadoVenta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoVenta", x => x.IdEstadoVenta);
                });

            migrationBuilder.CreateIndex(
                name: "IX_venta_EstadoVentaId",
                table: "venta",
                column: "EstadoVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_venta_EstadoVenta_EstadoVentaId",
                table: "venta",
                column: "EstadoVentaId",
                principalTable: "EstadoVenta",
                principalColumn: "IdEstadoVenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_venta_EstadoVenta_EstadoVentaId",
                table: "venta");

            migrationBuilder.DropTable(
                name: "EstadoVenta");

            migrationBuilder.DropIndex(
                name: "IX_venta_EstadoVentaId",
                table: "venta");

            migrationBuilder.DropColumn(
                name: "EstadoVentaId",
                table: "venta");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "cliente");
        }
    }
}
