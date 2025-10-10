using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChatLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Mes = table.Column<string>(type: "TEXT", nullable: false),
                    SendDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Preview = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    BookId = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatLogs_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Name",
                table: "Books",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatLogs_BookId",
                table: "ChatLogs",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLogs_Name",
                table: "ChatLogs",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ChatLogs_SendDate",
                table: "ChatLogs",
                column: "SendDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatLogs");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
