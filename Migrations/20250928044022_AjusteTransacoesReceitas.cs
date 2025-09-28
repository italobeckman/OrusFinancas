using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class AjusteTransacoesReceitas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacao_Assinatura_AssinaturaId",
                table: "Transacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Transacao_Categoria_CategoriaId",
                table: "Transacao");

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaId",
                table: "Transacao",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TipoReceita",
                table: "Transacao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoCategoria",
                table: "Categoria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Transacao_Assinatura_AssinaturaId",
                table: "Transacao",
                column: "AssinaturaId",
                principalTable: "Assinatura",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transacao_Categoria_CategoriaId",
                table: "Transacao",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacao_Assinatura_AssinaturaId",
                table: "Transacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Transacao_Categoria_CategoriaId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "TipoReceita",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "TipoCategoria",
                table: "Categoria");

            migrationBuilder.AlterColumn<int>(
                name: "CategoriaId",
                table: "Transacao",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transacao_Assinatura_AssinaturaId",
                table: "Transacao",
                column: "AssinaturaId",
                principalTable: "Assinatura",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacao_Categoria_CategoriaId",
                table: "Transacao",
                column: "CategoriaId",
                principalTable: "Categoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
