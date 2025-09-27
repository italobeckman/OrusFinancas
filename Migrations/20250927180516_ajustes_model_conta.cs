using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class ajustes_model_conta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta");

            migrationBuilder.RenameColumn(
                name: "SaldoAtual",
                table: "Conta",
                newName: "SaldoInicial");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Conta",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Conta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContaId",
                table: "Assinatura",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assinatura_ContaId",
                table: "Assinatura",
                column: "ContaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinatura_Conta_ContaId",
                table: "Assinatura",
                column: "ContaId",
                principalTable: "Conta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinatura_Conta_ContaId",
                table: "Assinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_Conta_Usuario_UsuarioId",
                table: "Conta");

            migrationBuilder.DropIndex(
                name: "IX_Assinatura_ContaId",
                table: "Assinatura");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Conta");

            migrationBuilder.DropColumn(
                name: "ContaId",
                table: "Assinatura");

            migrationBuilder.RenameColumn(
                name: "SaldoInicial",
                table: "Conta",
                newName: "SaldoAtual");

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
    }
}
