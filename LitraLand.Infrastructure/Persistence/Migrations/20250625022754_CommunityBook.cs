using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommunityBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CommunityBooks_Price",
                table: "CommunityBooks");

            migrationBuilder.DropColumn(
                name: "IsForSale",
                table: "CommunityBooks");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "CommunityBooks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "CommunityBooks",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "IsForSale",
                table: "CommunityBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommunityBooks_Price",
                table: "CommunityBooks",
                sql: "IsForSale = 0 OR (IsForSale = 1 AND Price IS NOT NULL)");
        }
    }
}
