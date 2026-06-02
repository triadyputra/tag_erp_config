using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tagApiKonfigurasi.Migrations
{
    /// <inheritdoc />
    public partial class actions_composite_pk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Actions",
                table: "Actions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actions",
                table: "Actions",
                columns: new[] { "IdController", "IdAction" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Actions",
                table: "Actions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actions",
                table: "Actions",
                column: "IdAction");
        }
    }
}
