using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevProjectServer.Migrations
{
    /// <inheritdoc />
    public partial class UseUserProfileAsIdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GitProjects_UserProfiles_UserProfileId",
                table: "GitProjects");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_GitProjects_UserProfileId",
                table: "GitProjects");

            migrationBuilder.AddColumn<string>(
                name: "UserProfileId1",
                table: "GitProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GithubUsername",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GitProjects_UserProfileId1",
                table: "GitProjects",
                column: "UserProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GitProjects_AspNetUsers_UserProfileId1",
                table: "GitProjects",
                column: "UserProfileId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GitProjects_AspNetUsers_UserProfileId1",
                table: "GitProjects");

            migrationBuilder.DropIndex(
                name: "IX_GitProjects_UserProfileId1",
                table: "GitProjects");

            migrationBuilder.DropColumn(
                name: "UserProfileId1",
                table: "GitProjects");

            migrationBuilder.DropColumn(
                name: "GithubUsername",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GithubUsername = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GitProjects_UserProfileId",
                table: "GitProjects",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_GitProjects_UserProfiles_UserProfileId",
                table: "GitProjects",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
