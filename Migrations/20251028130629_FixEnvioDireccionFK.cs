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

            //3) Borrar constraint por defecto y luego la columna fantasma si existe
            migrationBuilder.Sql(@"
            IF COL_LENGTH('envio','IdDireccionNavigationIdDireccion') IS NOT NULL
            BEGIN
                DECLARE @ConstraintName NVARCHAR(200);
                SELECT @ConstraintName = d.name
                FROM sys.default_constraints d
                JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
                WHERE d.parent_object_id = OBJECT_ID('envio') AND c.name = 'IdDireccionNavigationIdDireccion';

                IF @ConstraintName IS NOT NULL
                    EXEC('ALTER TABLE envio DROP CONSTRAINT ' + @ConstraintName);

                ALTER TABLE envio DROP COLUMN IdDireccionNavigationIdDireccion;
            END
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
