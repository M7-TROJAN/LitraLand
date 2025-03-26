using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_ImagePublicId_Column_To_Subscriber_Moddel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Subscribers");
        }
    }
}
