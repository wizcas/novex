using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Novex.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChatLogAnalysisRuleBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatLogAnalysisRuleBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatLogAnalysisRuleBooks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatLogAnalysisRuleBooks_Name",
                table: "ChatLogAnalysisRuleBooks",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatLogAnalysisRuleBooks");
        }
    }
}
