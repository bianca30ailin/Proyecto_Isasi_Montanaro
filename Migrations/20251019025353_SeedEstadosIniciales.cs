using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class SeedEstadosIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "estado",
                columns: new[] { "id_estado", "Descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Pedido confirmado, preparando para envío", "En preparación" },
                    { 2, "Pedido despachado, en tránsito", "En camino" },
                    { 3, "Pedido recibido por el cliente", "Entregado" },
                    { 4, "A la espera de procesamiento", "Pendiente" },
                    { 5, "Envío cancelado antes del despacho", "Cancelado" },
                    { 6, "El pedido fue devuelto al origen", "Devuelto" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "estado",
                keyColumn: "id_estado",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "estado",
                keyColumn: "id_estado",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "estado",
                keyColumn: "id_estado",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "estado",
                keyColumn: "id_estado",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "estado",
                keyColumn: "id_estado",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "estado",
                keyColumn: "id_estado",
                keyValue: 6);
        }
    }
}
