using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class CambiarRelacionClienteDireccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "direccion_cliente");

            migrationBuilder.AddColumn<int>(
                name: "dni_cliente",
                table: "direccion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_direccion_dni_cliente",
                table: "direccion",
                column: "dni_cliente");

            migrationBuilder.AddForeignKey(
                name: "FK_direccion_cliente",
                table: "direccion",
                column: "dni_cliente",
                principalTable: "cliente",
                principalColumn: "dni_cliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_direccion_cliente",
                table: "direccion");

            migrationBuilder.DropIndex(
                name: "IX_direccion_dni_cliente",
                table: "direccion");

            migrationBuilder.DropColumn(
                name: "dni_cliente",
                table: "direccion");

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

            migrationBuilder.CreateIndex(
                name: "IX_direccion_cliente_dni_cliente",
                table: "direccion_cliente",
                column: "dni_cliente");
        }
    }
}
