using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTerminyLekarzy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "termin_lekarza",
                columns: table => new
                {
                    id_terminu = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_rozpoczecia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_zakonczenia = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    czy_dostepny = table.Column<bool>(type: "boolean", nullable: false),
                    id_lekarza = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_termin_lekarza", x => x.id_terminu);
                    table.ForeignKey(
                        name: "FK_termin_lekarza_lekarz_id_lekarza",
                        column: x => x.id_lekarza,
                        principalTable: "lekarz",
                        principalColumn: "id_lekarza",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_termin_lekarza_czy_dostepny",
                table: "termin_lekarza",
                column: "czy_dostepny");

            migrationBuilder.CreateIndex(
                name: "IX_termin_lekarza_data_rozpoczecia",
                table: "termin_lekarza",
                column: "data_rozpoczecia");

            migrationBuilder.CreateIndex(
                name: "IX_termin_lekarza_id_lekarza",
                table: "termin_lekarza",
                column: "id_lekarza");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "termin_lekarza");
        }
    }
}
