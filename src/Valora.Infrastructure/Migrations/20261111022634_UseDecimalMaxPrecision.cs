using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Valora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseDecimalMaxPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "default_amount",
                table: "recurrence_rule_versions",
                type: "numeric(31,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(29,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "entries",
                type: "numeric(31,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(29,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "default_amount",
                table: "recurrence_rule_versions",
                type: "numeric(29,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(31,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "amount",
                table: "entries",
                type: "numeric(29,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(31,2)",
                oldNullable: true);
        }
    }
}
