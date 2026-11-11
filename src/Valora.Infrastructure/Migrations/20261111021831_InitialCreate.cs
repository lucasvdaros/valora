using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recurrence_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    start_month = table.Column<int>(type: "integer", nullable: false),
                    end_month = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurrence_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recurrence_rule_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    effective_from_month = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    value_mode = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    default_amount = table.Column<decimal>(type: "numeric(29,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurrence_rule_versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recurrence_rule_versions_recurrence_rules_rule_id",
                        column: x => x.rule_id,
                        principalTable: "recurrence_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    month_key = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(29,2)", nullable: true),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    recurrence_rule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recurrence_rule_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entries_recurrence_rule_versions_recurrence_rule_version_id",
                        column: x => x.recurrence_rule_version_id,
                        principalTable: "recurrence_rule_versions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entries_recurrence_rules_recurrence_rule_id",
                        column: x => x.recurrence_rule_id,
                        principalTable: "recurrence_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entries_month_key",
                table: "entries",
                column: "month_key");

            migrationBuilder.CreateIndex(
                name: "IX_entries_recurrence_rule_id_month_key",
                table: "entries",
                columns: new[] { "recurrence_rule_id", "month_key" },
                unique: true,
                filter: "\"recurrence_rule_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_entries_recurrence_rule_version_id",
                table: "entries",
                column: "recurrence_rule_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurrence_rule_versions_rule_id_effective_from_month",
                table: "recurrence_rule_versions",
                columns: new[] { "rule_id", "effective_from_month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entries");

            migrationBuilder.DropTable(
                name: "recurrence_rule_versions");

            migrationBuilder.DropTable(
                name: "recurrence_rules");
        }
    }
}
