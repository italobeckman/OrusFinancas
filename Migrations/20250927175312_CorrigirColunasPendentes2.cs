using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirColunasPendentes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Conta",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Conta",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
