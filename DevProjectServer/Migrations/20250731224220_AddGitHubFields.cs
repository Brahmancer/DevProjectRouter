using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevProjectServer.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReadmeHtml",
                table: "GitProjects",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "GitHubAccessToken",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GitHubTokenExpiresAt",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubAccessToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GitHubTokenExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ReadmeHtml",
                table: "GitProjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
