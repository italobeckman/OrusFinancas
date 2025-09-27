using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class AjusteAssinaturaUnidirecional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categoria_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeBanco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conta_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InsightFinanceiro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Detalhe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsightFinanceiro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsightFinanceiro_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orcamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorLimite = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orcamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orcamento_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilFinanceiro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RendaMensal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Metas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilFinanceiro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilFinanceiro_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataTransacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transacao_Categoria_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transacao_Conta_ContaId",
                        column: x => x.ContaId,
                        principalTable: "Conta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assinatura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Servico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValorMensal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    DespesaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinatura_Transacao_DespesaId",
                        column: x => x.DespesaId,
                        principalTable: "Transacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assinatura_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assinatura_DespesaId",
                table: "Assinatura",
                column: "DespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinatura_UsuarioId",
                table: "Assinatura",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Categoria_UsuarioId",
                table: "Categoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conta_UsuarioId",
                table: "Conta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_InsightFinanceiro_UsuarioId",
                table: "InsightFinanceiro",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Orcamento_UsuarioId",
                table: "Orcamento",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilFinanceiro_UsuarioId",
                table: "PerfilFinanceiro",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacao_CategoriaId",
                table: "Transacao",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacao_ContaId",
                table: "Transacao",
                column: "ContaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assinatura");

            migrationBuilder.DropTable(
                name: "InsightFinanceiro");

            migrationBuilder.DropTable(
                name: "Orcamento");

            migrationBuilder.DropTable(
                name: "PerfilFinanceiro");

            migrationBuilder.DropTable(
                name: "Transacao");

            migrationBuilder.DropTable(
                name: "Categoria");

            migrationBuilder.DropTable(
                name: "Conta");
        }
    }
}
