using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class FixEnvioDireccionFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Borrar FK mala si existe
            migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_envio_direccion_IdDireccionNavigationIdDireccion')
                ALTER TABLE envio DROP CONSTRAINT FK_envio_direccion_IdDireccionNavigationIdDireccion;
            ");

            // 2) Borrar índice malo si existe
            migrationBuilder.Sql(@"
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_envio_IdDireccionNavigationIdDireccion' AND object_id = OBJECT_ID('envio'))
                DROP INDEX IX_envio_IdDireccionNavigationIdDireccion ON envio;
            ");

            // 3) Borrar columna fantasma si existe
            migrationBuilder.Sql(@"
            IF COL_LENGTH('envio','IdDireccionNavigationIdDireccion') IS NOT NULL
                ALTER TABLE envio DROP COLUMN IdDireccionNavigationIdDireccion;
            ");

            // 4) Crear índice correcto si NO existe
            migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_envio_IdDireccion' AND object_id = OBJECT_ID('envio'))
                CREATE INDEX IX_envio_IdDireccion ON envio(IdDireccion);
            ");

            // 5) Crear FK correcta si NO existe
            migrationBuilder.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_envio_direccion_IdDireccion')
                ALTER TABLE envio
                ADD CONSTRAINT FK_envio_direccion_IdDireccion
                FOREIGN KEY (IdDireccion) REFERENCES direccion(id_direccion) ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
