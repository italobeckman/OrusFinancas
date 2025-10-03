using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class Inicial_Limpa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinatura_Transacao_DespesaId",
                table: "Assinatura");

            migrationBuilder.AlterColumn<int>(
                name: "DespesaId",
                table: "Assinatura",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinatura_Transacao_DespesaId",
                table: "Assinatura",
                column: "DespesaId",
                principalTable: "Transacao",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinatura_Transacao_DespesaId",
                table: "Assinatura");

            migrationBuilder.AlterColumn<int>(
                name: "DespesaId",
                table: "Assinatura",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assinatura_Transacao_DespesaId",
                table: "Assinatura",
                column: "DespesaId",
                principalTable: "Transacao",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
