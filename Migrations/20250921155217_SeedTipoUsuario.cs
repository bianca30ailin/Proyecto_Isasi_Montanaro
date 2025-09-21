using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class SeedTipoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 4);
        }
    }
}
