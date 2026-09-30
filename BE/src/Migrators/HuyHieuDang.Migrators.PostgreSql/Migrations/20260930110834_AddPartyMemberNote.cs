using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuyHieuDang.Migrators.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddPartyMemberNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "party_member",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NoteUpdatedAt",
                table: "party_member",
                type: "timestamptz",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "party_member");

            migrationBuilder.DropColumn(
                name: "NoteUpdatedAt",
                table: "party_member");
        }
    }
}
