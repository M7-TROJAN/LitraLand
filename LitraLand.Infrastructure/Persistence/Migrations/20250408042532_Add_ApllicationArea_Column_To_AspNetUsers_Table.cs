using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_ApllicationArea_Column_To_AspNetUsers_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApllicationArea",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApllicationArea",
                table: "AspNetUsers");
        }
    }
}
