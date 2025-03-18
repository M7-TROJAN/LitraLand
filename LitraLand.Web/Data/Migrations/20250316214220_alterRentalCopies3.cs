using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class alterRentalCopies3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RentalCopies",
                table: "RentalCopies");

            migrationBuilder.DropIndex(
                name: "IX_RentalCopies_RentalId",
                table: "RentalCopies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RentalCopies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RentalCopies",
                table: "RentalCopies",
                columns: new[] { "RentalId", "BookCopyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RentalCopies",
                table: "RentalCopies");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RentalCopies",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RentalCopies",
                table: "RentalCopies",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RentalCopies_RentalId",
                table: "RentalCopies",
                column: "RentalId");
        }
    }
}
