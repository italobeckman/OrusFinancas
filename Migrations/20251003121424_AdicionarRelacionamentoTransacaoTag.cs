using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRelacionamentoTransacaoTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransacaoTag",
                columns: table => new
                {
                    TransacaoId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    DataAssociacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransacaoTag", x => new { x.TransacaoId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TransacaoTag_Tag_TagId",
                        column: x => x.TagId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransacaoTag_Transacao_TransacaoId",
                        column: x => x.TransacaoId,
                        principalTable: "Transacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_UsuarioId",
                table: "Tag",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TransacaoTag_TagId",
                table: "TransacaoTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransacaoTag");

            migrationBuilder.DropTable(
                name: "Tag");
        }
    }
}
