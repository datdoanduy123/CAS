using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationCodes_Clients_ClientId",
                table: "AuthorizationCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Clients_ClientId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Clients_ClientId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "ClientSessions");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Roles",
                newName: "AppId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_ClientId",
                table: "Roles",
                newName: "IX_Roles_AppId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "RefreshTokens",
                newName: "AppId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_ClientId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_AppId");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "AuthorizationCodes",
                newName: "AppId");

            migrationBuilder.RenameIndex(
                name: "IX_AuthorizationCodes_ClientId",
                table: "AuthorizationCodes",
                newName: "IX_AuthorizationCodes_AppId");

            migrationBuilder.CreateTable(
                name: "Apps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    AppSecret = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RedirectUris = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AllowedGrantTypes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LogoutUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AppType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RealmId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Apps_Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSessions_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppSessions_UserSessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apps_RealmId",
                table: "Apps",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_AppId",
                table: "AppSessions",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSessions_UserSessionId",
                table: "AppSessions",
                column: "UserSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorizationCodes_Apps_AppId",
                table: "AuthorizationCodes",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Apps_AppId",
                table: "RefreshTokens",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Apps_AppId",
                table: "Roles",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuthorizationCodes_Apps_AppId",
                table: "AuthorizationCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Apps_AppId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Apps_AppId",
                table: "Roles");

            migrationBuilder.DropTable(
                name: "AppSessions");

            migrationBuilder.DropTable(
                name: "Apps");

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "Roles",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_AppId",
                table: "Roles",
                newName: "IX_Roles_ClientId");

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "RefreshTokens",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_AppId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_ClientId");

            migrationBuilder.RenameColumn(
                name: "AppId",
                table: "AuthorizationCodes",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_AuthorizationCodes_AppId",
                table: "AuthorizationCodes",
                newName: "IX_AuthorizationCodes_ClientId");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RealmId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AllowedGrantTypes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ClientSecret = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ClientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LogoutUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RedirectUris = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientSessions_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientSessions_UserSessions_UserSessionId",
                        column: x => x.UserSessionId,
                        principalTable: "UserSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_RealmId",
                table: "Clients",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSessions_ClientId",
                table: "ClientSessions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientSessions_UserSessionId",
                table: "ClientSessions",
                column: "UserSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuthorizationCodes_Clients_ClientId",
                table: "AuthorizationCodes",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Clients_ClientId",
                table: "RefreshTokens",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Clients_ClientId",
                table: "Roles",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
