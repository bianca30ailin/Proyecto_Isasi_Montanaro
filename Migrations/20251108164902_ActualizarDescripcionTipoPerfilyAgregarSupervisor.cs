using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarDescripcionTipoPerfilyAgregarSupervisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 1,
                column: "descripcion",
                value: "Usuario con acceso total a usuario y solo lectura para las demas áreas");

            migrationBuilder.UpdateData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 3,
                column: "descripcion",
                value: "Usuario que se encarga de actualizar o registrar datos de los envios");

            migrationBuilder.InsertData(
                table: "tipo_usuario",
                columns: new[] { "id_tipo_usuario", "descripcion", "tipo" },
                values: new object[] { 5, "Usuario con permisos para generar informes", "Supervisor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 1,
                column: "descripcion",
                value: "Usuario con acceso total al sistema");

            migrationBuilder.UpdateData(
                table: "tipo_usuario",
                keyColumn: "id_tipo_usuario",
                keyValue: 3,
                column: "descripcion",
                value: "Usuario que se encarga de registrar los envíos y sus estados");
        }
    }
}
