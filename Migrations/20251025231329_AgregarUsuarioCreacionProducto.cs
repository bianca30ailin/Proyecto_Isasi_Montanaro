using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Isasi_Montanaro.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuarioCreacionProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "producto",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "id_usuario_creacion",
                table: "producto",
                type: "int",
                nullable: true);

            // Actualizamos productos existentes con el primer usuario administrador
            // (Puedes cambiar el WHERE para seleccionar un usuario específico)
            migrationBuilder.Sql(@"
                UPDATE producto 
                SET id_usuario_creacion = (
                    SELECT TOP 1 u.id_usuario 
                    FROM usuario u 
                    INNER JOIN funcion_usuario fu ON u.id_usuario = fu.id_usuario 
                    WHERE fu.id_tipo_usuario = 1
                    ORDER BY u.id_usuario
                )
                    WHERE id_usuario_creacion IS NULL
                ");

            // Ahora hacemos la columna NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "id_usuario_creacion",
                table: "producto",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_producto_id_usuario_creacion",
                table: "producto",
                column: "id_usuario_creacion");

            migrationBuilder.AddForeignKey(
                name: "FK_producto_usuario_creacion",
                table: "producto",
                column: "id_usuario_creacion",
                principalTable: "usuario",
                principalColumn: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producto_usuario_creacion",
                table: "producto");

            migrationBuilder.DropIndex(
                name: "IX_producto_id_usuario_creacion",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "id_usuario_creacion",
                table: "producto");
        }
    }
}
