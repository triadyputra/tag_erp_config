using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tagApiKonfigurasi.Migrations
{
    /// <inheritdoc />
    public partial class add_niksistag_idmodul_to_aspnetusers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdModul",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NikSistag",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IdModul",
                table: "AspNetUsers",
                column: "IdModul");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Moduls_IdModul",
                table: "AspNetUsers",
                column: "IdModul",
                principalTable: "Moduls",
                principalColumn: "IdModul",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                "UPDATE AspNetUsers SET NikSistag = UserName WHERE NikSistag = '' OR NikSistag IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Moduls_IdModul",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IdModul",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdModul",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NikSistag",
                table: "AspNetUsers");
        }
    }
}
