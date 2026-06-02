using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tagApiKonfigurasi.Migrations
{
    /// <inheritdoc />
    public partial class add_user_noktp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoKtp",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoKtp",
                table: "AspNetUsers");
        }
    }
}
