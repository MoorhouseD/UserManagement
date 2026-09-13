using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserManagement.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserActionLogs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserActionLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Forename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                NormalizedEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "DateOfBirth", "Email", "Forename", "IsActive", "NormalizedEmail", "Surname" },
            values: new object[,]
            {
                { 1L, new DateOnly(1978, 4, 12), "ploew@example.com", "Peter", true, "PLOEW@EXAMPLE.COM", "Loew" },
                { 2L, new DateOnly(1965, 10, 28), "bfgates@example.com", "Benjamin Franklin", true, "BFGATES@EXAMPLE.COM", "Gates" },
                { 3L, new DateOnly(1989, 2, 7), "ctroy@example.com", "Castor", false, "CTROY@EXAMPLE.COM", "Troy" },
                { 4L, new DateOnly(1972, 8, 19), "mraines@example.com", "Memphis", true, "MRAINES@EXAMPLE.COM", "Raines" },
                { 5L, new DateOnly(1958, 12, 3), "sgodspeed@example.com", "Stanley", true, "SGODSPEED@EXAMPLE.COM", "Goodspeed" },
                { 6L, new DateOnly(1981, 6, 25), "himcdunnough@example.com", "H.I.", true, "HIMCDUNNOUGH@EXAMPLE.COM", "McDunnough" },
                { 7L, new DateOnly(1992, 1, 16), "cpoe@example.com", "Cameron", false, "CPOE@EXAMPLE.COM", "Poe" },
                { 8L, new DateOnly(1986, 9, 30), "emalus@example.com", "Edward", false, "EMALUS@EXAMPLE.COM", "Malus" },
                { 9L, new DateOnly(1975, 3, 21), "dmacready@example.com", "Damon", false, "DMACREADY@EXAMPLE.COM", "Macready" },
                { 10L, new DateOnly(1990, 11, 8), "jblaze@example.com", "Johnny", true, "JBLAZE@EXAMPLE.COM", "Blaze" },
                { 11L, new DateOnly(1983, 7, 14), "rfeld@example.com", "Robin", true, "RFELD@EXAMPLE.COM", "Feld" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_NormalizedEmail",
            table: "Users",
            column: "NormalizedEmail",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserActionLogs");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
