using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePKCEFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeChallenge",
                table: "AuthorizationCodes");

            migrationBuilder.DropColumn(
                name: "CodeChallengeMethod",
                table: "AuthorizationCodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeChallenge",
                table: "AuthorizationCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeChallengeMethod",
                table: "AuthorizationCodes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
