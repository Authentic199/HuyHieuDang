using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuyHieuDang.Migrators.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ModelType = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: false),
                    Guards = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "citext", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ModelType = table.Column<string>(type: "text", nullable: false),
                    Guards = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "model_permission",
                columns: table => new
                {
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelType = table.Column<string>(type: "text", nullable: false),
                    PermissionCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_permission", x => new { x.ModelId, x.ModelType, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_model_permission_permission_PermissionCode",
                        column: x => x.PermissionCode,
                        principalTable: "permission",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "model_role",
                columns: table => new
                {
                    ModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelType = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_role", x => new { x.ModelId, x.ModelType, x.RoleId });
                    table.ForeignKey(
                        name: "FK_model_role_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permission", x => new { x.RoleId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_role_permission_permission_PermissionCode",
                        column: x => x.PermissionCode,
                        principalTable: "permission",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permission_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "citext", nullable: false),
                    Email = table.Column<string>(type: "citext", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    DayOfBirth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_refresh_token",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpireTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_refresh_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_refresh_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_model_permission_PermissionCode",
                table: "model_permission",
                column: "PermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_model_role_RoleId",
                table: "model_role",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_role_Code",
                table: "role",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permission_PermissionCode",
                table: "role_permission",
                column: "PermissionCode");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_RoleId",
                table: "user",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_user_Username",
                table: "user",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_refresh_token_Token",
                table: "user_refresh_token",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_refresh_token_UserId",
                table: "user_refresh_token",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "model_permission");

            migrationBuilder.DropTable(
                name: "model_role");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "user_refresh_token");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "role");
        }
    }
}
