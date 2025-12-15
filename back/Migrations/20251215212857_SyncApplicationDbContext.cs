using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back.Migrations
{
    /// <inheritdoc />
    public partial class SyncApplicationDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlaybackPosition",
                table: "Teams",
                newName: "ElapsedSeconds");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAtUtc",
                table: "Teams",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedAtUtc",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "ElapsedSeconds",
                table: "Teams",
                newName: "PlaybackPosition");
        }
    }
}
