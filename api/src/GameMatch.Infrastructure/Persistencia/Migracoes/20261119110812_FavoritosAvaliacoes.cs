using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMatch.Infrastructure.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class FavoritosAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avaliacoes",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    jogo_id = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<int>(type: "integer", nullable: false),
                    atualizada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacoes", x => new { x.usuario_id, x.jogo_id });
                    table.CheckConstraint("ck_avaliacoes_nota", "nota BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_avaliacoes_AspNetUsers_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_avaliacoes_jogos_jogo_id",
                        column: x => x.jogo_id,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favoritos",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    jogo_id = table.Column<int>(type: "integer", nullable: false),
                    criado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favoritos", x => new { x.usuario_id, x.jogo_id });
                    table.ForeignKey(
                        name: "FK_favoritos_AspNetUsers_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favoritos_jogos_jogo_id",
                        column: x => x.jogo_id,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_jogo_id",
                table: "avaliacoes",
                column: "jogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_usuario_id",
                table: "avaliacoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_favoritos_jogo_id",
                table: "favoritos",
                column: "jogo_id");

            migrationBuilder.CreateIndex(
                name: "IX_favoritos_usuario_id",
                table: "favoritos",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacoes");

            migrationBuilder.DropTable(
                name: "favoritos");
        }
    }
}
