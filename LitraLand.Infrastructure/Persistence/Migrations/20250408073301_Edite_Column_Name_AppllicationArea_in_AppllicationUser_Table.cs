using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Edite_Column_Name_AppllicationArea_in_AppllicationUser_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApllicationArea",
                table: "AspNetUsers",
                newName: "AppllicationArea");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppllicationArea",
                table: "AspNetUsers",
                newName: "ApllicationArea");
        }
    }
}
