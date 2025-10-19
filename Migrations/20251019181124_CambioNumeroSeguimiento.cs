using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class CambioNumeroSeguimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "num_seguimiento",
                table: "envio",
                type: "varchar(25)",
                unicode: false,
                maxLength: 25,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(25)",
                oldUnicode: false,
                oldMaxLength: 25);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "num_seguimiento",
                table: "envio",
                type: "varchar(25)",
                unicode: false,
                maxLength: 25,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(25)",
                oldUnicode: false,
                oldMaxLength: 25,
                oldNullable: true);
        }
    }
}
