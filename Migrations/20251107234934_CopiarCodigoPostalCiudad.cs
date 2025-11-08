using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class CopiarCodigoPostalCiudad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Copiar los códigos postales existentes desde Direccion hacia Ciudad
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.CodigoPostal = d.codigo_postal
                FROM Ciudad c
                INNER JOIN Direccion d ON c.id_ciudad = d.id_ciudad
                WHERE c.CodigoPostal = 0 OR c.CodigoPostal IS NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No es necesario revertir la copia
        }
    }
}
