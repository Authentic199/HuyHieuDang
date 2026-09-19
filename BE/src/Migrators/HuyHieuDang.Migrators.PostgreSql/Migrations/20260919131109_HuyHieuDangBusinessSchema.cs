using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuyHieuDang.Migrators.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class HuyHieuDangBusinessSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_setting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartYears = table.Column<int>(type: "integer", nullable: false),
                    EndYears = table.Column<int>(type: "integer", nullable: false),
                    StepYears = table.Column<int>(type: "integer", nullable: false),
                    UnitName = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "award_period",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "citext", nullable: false),
                    FromDay = table.Column<int>(type: "integer", nullable: false),
                    FromMonth = table.Column<int>(type: "integer", nullable: false),
                    ToDay = table.Column<int>(type: "integer", nullable: false),
                    ToMonth = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_award_period", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "party_member",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false, collation: "vi-x-icu"),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<byte>(type: "smallint", nullable: true),
                    OfficialAdmissionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_party_member", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_award_period_Name",
                table: "award_period",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_party_member_OfficialAdmissionDate",
                table: "party_member",
                column: "OfficialAdmissionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_setting");

            migrationBuilder.DropTable(
                name: "award_period");

            migrationBuilder.DropTable(
                name: "party_member");
        }
    }
}
