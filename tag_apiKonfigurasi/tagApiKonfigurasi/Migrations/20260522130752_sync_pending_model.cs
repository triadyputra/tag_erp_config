using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tagApiKonfigurasi.Migrations
{
    /// <inheritdoc />
    public partial class sync_pending_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Actions_IdController",
                table: "Actions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Actions_IdController",
                table: "Actions",
                column: "IdController");
        }
    }
}
