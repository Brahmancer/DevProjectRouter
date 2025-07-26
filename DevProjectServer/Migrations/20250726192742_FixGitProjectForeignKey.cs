using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevProjectServer.Migrations
{
    /// <inheritdoc />
    public partial class FixGitProjectForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "UserProfileId",
                table: "GitProjects",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "GithubUsername",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_GitProjects_UserProfileId",
                table: "GitProjects",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_GitProjects_AspNetUsers_UserProfileId",
                table: "GitProjects",
                column: "UserProfileId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GitProjects_AspNetUsers_UserProfileId",
                table: "GitProjects");

            migrationBuilder.DropIndex(
                name: "IX_GitProjects_UserProfileId",
                table: "GitProjects");

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "GitProjects",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "UserProfileId1",
                table: "GitProjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GithubUsername",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

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
    }
}
