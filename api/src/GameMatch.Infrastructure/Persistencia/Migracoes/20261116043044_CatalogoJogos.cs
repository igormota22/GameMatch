using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameMatch.Infrastructure.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class CatalogoJogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jogos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_externo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    thumbnail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    desenvolvedor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    url_fonte = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sincronizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jogos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jogos_caracteristicas",
                columns: table => new
                {
                    jogo_id = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jogos_caracteristicas", x => new { x.jogo_id, x.tipo, x.valor });
                    table.ForeignKey(
                        name: "FK_jogos_caracteristicas_jogos_jogo_id",
                        column: x => x.jogo_id,
                        principalTable: "jogos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jogos_id_externo",
                table: "jogos",
                column: "id_externo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_jogos_caracteristicas_tipo_valor",
                table: "jogos_caracteristicas",
                columns: new[] { "tipo", "valor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "jogos_caracteristicas");

            migrationBuilder.DropTable(
                name: "jogos");
        }
    }
}
