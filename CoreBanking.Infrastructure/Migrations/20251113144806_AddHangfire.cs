using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHangfire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInterestBearing",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityDate",
                table: "Accounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                columns: new[] { "IsArchived", "IsInterestBearing", "LastActivityDate", "Status" },
                values: new object[] { false, false, new DateTime(2025, 11, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsInterestBearing",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Accounts");
        }
    }
}
