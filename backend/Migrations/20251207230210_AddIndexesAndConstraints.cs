using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmotionRecords_CreatedAt",
                table: "EmotionRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionRecords_Label",
                table: "EmotionRecords",
                column: "Label");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionRecords_Username",
                table: "EmotionRecords",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmotionRecords_CreatedAt",
                table: "EmotionRecords");

            migrationBuilder.DropIndex(
                name: "IX_EmotionRecords_Label",
                table: "EmotionRecords");

            migrationBuilder.DropIndex(
                name: "IX_EmotionRecords_Username",
                table: "EmotionRecords");
        }
    }
}
