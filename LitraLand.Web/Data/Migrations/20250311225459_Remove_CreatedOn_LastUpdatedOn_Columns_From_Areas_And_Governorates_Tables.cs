using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LitraLand.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class Remove_CreatedOn_LastUpdatedOn_Columns_From_Areas_And_Governorates_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "LastUpdatedOn",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "LastUpdatedOn",
                table: "Areas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Governorates",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedOn",
                table: "Governorates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Areas",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedOn",
                table: "Areas",
                type: "datetime2",
                nullable: true);
        }
    }
}
