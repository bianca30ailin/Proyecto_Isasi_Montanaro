using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class SeedEstadoVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EstadoVenta",
                columns: new[] { "IdEstadoVenta", "Descripcion", "Motivo", "NombreEstado" },
                values: new object[,]
                {
                    { 1, "Venta confirmada y en curso", null, "Activa" },
                    { 2, "Venta registrada, esperando confirmación del pago", null, "Pendiente de pago" },
                    { 3, "Venta anulada por el cliente o vendedor", null, "Cancelada" },
                    { 4, "Venta finalizada con entrega y pago confirmados", null, "Completada" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EstadoVenta",
                keyColumn: "IdEstadoVenta",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EstadoVenta",
                keyColumn: "IdEstadoVenta",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EstadoVenta",
                keyColumn: "IdEstadoVenta",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EstadoVenta",
                keyColumn: "IdEstadoVenta",
                keyValue: 4);
        }
    }
}
