using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMatch.Infrastructure.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class PerfilGamer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "perfis_gamer",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    atualizado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis_gamer", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_perfis_gamer_AspNetUsers_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "perfis_preferencias",
                columns: table => new
                {
                    usuario_id = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfis_preferencias", x => new { x.usuario_id, x.tipo, x.valor });
                    table.ForeignKey(
                        name: "FK_perfis_preferencias_perfis_gamer_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "perfis_gamer",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "perfis_preferencias");

            migrationBuilder.DropTable(
                name: "perfis_gamer");
        }
    }
}
