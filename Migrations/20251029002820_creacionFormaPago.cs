using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class creacionFormaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_forma_pago",
                table: "venta",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "total_cuotas",
                table: "venta",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "forma_pago",
                columns: table => new
                {
                    id_forma_pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__forma_pa__DD4926E39FCE6D1E", x => x.id_forma_pago);
                });

            migrationBuilder.InsertData(
                table: "forma_pago",
                columns: new[] { "id_forma_pago", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "Pago en efectivo", "Efectivo" },
                    { 2, "Pago con tarjeta de débito", "Débito" },
                    { 3, "Pago con tarjeta de crédito con 10% de recargo", "Crédito" },
                    { 4, "Pago mediante transferencia bancaria", "Transferencia" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_venta_id_forma_pago",
                table: "venta",
                column: "id_forma_pago");

            migrationBuilder.AddForeignKey(
                name: "FK__venta__id_forma_pago",
                table: "venta",
                column: "id_forma_pago",
                principalTable: "forma_pago",
                principalColumn: "id_forma_pago",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__venta__id_forma_pago",
                table: "venta");

            migrationBuilder.DropTable(
                name: "forma_pago");

            migrationBuilder.DropIndex(
                name: "IX_venta_id_forma_pago",
                table: "venta");

            migrationBuilder.DropColumn(
                name: "id_forma_pago",
                table: "venta");

            migrationBuilder.DropColumn(
                name: "total_cuotas",
                table: "venta");
        }
    }
}
