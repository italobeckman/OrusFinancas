using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrusFinancas.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirColunasPendentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssinaturaId",
                table: "Transacao",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativa",
                table: "Assinatura",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAssinatura",
                table: "Assinatura",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Transacao_AssinaturaId",
                table: "Transacao",
                column: "AssinaturaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacao_Assinatura_AssinaturaId",
                table: "Transacao",
                column: "AssinaturaId",
                principalTable: "Assinatura",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacao_Assinatura_AssinaturaId",
                table: "Transacao");

            migrationBuilder.DropIndex(
                name: "IX_Transacao_AssinaturaId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "AssinaturaId",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "Ativa",
                table: "Assinatura");

            migrationBuilder.DropColumn(
                name: "DataAssinatura",
                table: "Assinatura");
        }
    }
}
