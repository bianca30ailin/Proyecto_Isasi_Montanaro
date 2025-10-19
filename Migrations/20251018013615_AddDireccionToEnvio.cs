using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class AddDireccionToEnvio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdDireccion",
                table: "envio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdDireccionNavigationIdDireccion",
                table: "envio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_envio_IdDireccionNavigationIdDireccion",
                table: "envio",
                column: "IdDireccionNavigationIdDireccion");

            migrationBuilder.AddForeignKey(
                name: "FK_envio_direccion_IdDireccionNavigationIdDireccion",
                table: "envio",
                column: "IdDireccionNavigationIdDireccion",
                principalTable: "direccion",
                principalColumn: "id_direccion",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_envio_direccion_IdDireccionNavigationIdDireccion",
                table: "envio");

            migrationBuilder.DropIndex(
                name: "IX_envio_IdDireccionNavigationIdDireccion",
                table: "envio");

            migrationBuilder.DropColumn(
                name: "IdDireccion",
                table: "envio");

            migrationBuilder.DropColumn(
                name: "IdDireccionNavigationIdDireccion",
                table: "envio");
        }
    }
}
