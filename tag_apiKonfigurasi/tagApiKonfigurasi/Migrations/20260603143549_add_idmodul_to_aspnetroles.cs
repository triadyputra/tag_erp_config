using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tagApiKonfigurasi.Migrations
{
    /// <inheritdoc />
    public partial class add_idmodul_to_aspnetroles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdModul",
                table: "AspNetRoles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_IdModul",
                table: "AspNetRoles",
                column: "IdModul");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_Moduls_IdModul",
                table: "AspNetRoles",
                column: "IdModul",
                principalTable: "Moduls",
                principalColumn: "IdModul",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                "UPDATE AspNetRoles SET IdModul = 'CONFIG' WHERE IdModul IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_Moduls_IdModul",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_IdModul",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IdModul",
                table: "AspNetRoles");
        }
    }
}
