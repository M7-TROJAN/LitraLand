using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Make_NationalID_PhoneNumber_Email_Unique_In_Subscribers_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_NationalId",
                table: "Subscribers",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_PhoneNumber",
                table: "Subscribers",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_NationalId",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_PhoneNumber",
                table: "Subscribers");
        }
    }
}
