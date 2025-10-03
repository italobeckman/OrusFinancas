using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class RemoverDespesaIdDeAssinatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinatura_Conta_ContaId",
                table: "Assinatura");

            migrationBuilder.DropForeignKey(
                name: "FK_Assinatura_Transacao_DespesaId",
                table: "Assinatura");

            migrationBuilder.DropIndex(
                name: "IX_Assinatura_DespesaId",
                table: "Assinatura");

            migrationBuilder.DropColumn(
                name: "DespesaId",
                table: "Assinatura");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinatura_Conta_ContaId",
                table: "Assinatura",
                column: "ContaId",
                principalTable: "Conta",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assinatura_Conta_ContaId",
                table: "Assinatura");

            migrationBuilder.AddColumn<int>(
                name: "DespesaId",
                table: "Assinatura",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assinatura_DespesaId",
                table: "Assinatura",
                column: "DespesaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinatura_Conta_ContaId",
                table: "Assinatura",
                column: "ContaId",
                principalTable: "Conta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assinatura_Transacao_DespesaId",
                table: "Assinatura",
                column: "DespesaId",
                principalTable: "Transacao",
                principalColumn: "Id");
        }
    }
}
