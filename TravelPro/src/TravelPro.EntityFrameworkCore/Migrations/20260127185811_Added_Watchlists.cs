using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelPro.Migrations
{
    /// <inheritdoc />
    public partial class Added_Watchlists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppWatchlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppWatchlists", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppWatchlists_UserId_DestinationId",
                table: "AppWatchlists",
                columns: new[] { "UserId", "DestinationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppWatchlists");
        }
    }
}
