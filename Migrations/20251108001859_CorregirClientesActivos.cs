using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class CorregirClientesActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Marcar todos los clientes existentes como activos
            migrationBuilder.Sql("UPDATE Cliente SET Activo = 1 WHERE Activo = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Si alguna vez se revierte, no hace falta cambiar nada
        }
    }
}
