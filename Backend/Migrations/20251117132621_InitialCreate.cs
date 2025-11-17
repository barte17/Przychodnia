using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lekarz",
                columns: table => new
                {
                    id_lekarza = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nazwisko = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    specjalizacja = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lekarz", x => x.id_lekarza);
                });

            migrationBuilder.CreateTable(
                name: "pacjent",
                columns: table => new
                {
                    id_pacjenta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nazwisko = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pesel = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pacjent", x => x.id_pacjenta);
                });

            migrationBuilder.CreateTable(
                name: "wizyta",
                columns: table => new
                {
                    id_wizyty = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    id_pacjenta = table.Column<int>(type: "integer", nullable: false),
                    id_lekarza = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wizyta", x => x.id_wizyty);
                    table.ForeignKey(
                        name: "FK_wizyta_lekarz_id_lekarza",
                        column: x => x.id_lekarza,
                        principalTable: "lekarz",
                        principalColumn: "id_lekarza",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wizyta_pacjent_id_pacjenta",
                        column: x => x.id_pacjenta,
                        principalTable: "pacjent",
                        principalColumn: "id_pacjenta",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pacjent_pesel",
                table: "pacjent",
                column: "pesel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wizyta_data",
                table: "wizyta",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "IX_wizyta_id_lekarza",
                table: "wizyta",
                column: "id_lekarza");

            migrationBuilder.CreateIndex(
                name: "IX_wizyta_id_pacjenta",
                table: "wizyta",
                column: "id_pacjenta");

            migrationBuilder.CreateIndex(
                name: "IX_wizyta_status",
                table: "wizyta",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wizyta");

            migrationBuilder.DropTable(
                name: "lekarz");

            migrationBuilder.DropTable(
                name: "pacjent");
        }
    }
}
