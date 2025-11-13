using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBanking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                column: "DateOpened",
                value: new DateTime(2024, 10, 10, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: new Guid("a1b2c3d4-1234-5678-9abc-123456789abc"),
                columns: new[] { "Address", "BVN", "CreditScore", "DateCreated", "DateOfBirth", "PhoneNumber" },
                values: new object[] { "123 Main Street, Lagos, Nigeria", "20000000009", 40, new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1995, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "555-0101" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "AccountId",
                keyValue: new Guid("c3d4e5f6-3456-7890-cde1-345678901cde"),
                column: "DateOpened",
                value: new DateTime(2025, 10, 18, 12, 24, 23, 715, DateTimeKind.Utc).AddTicks(643));

            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: new Guid("a1b2c3d4-1234-5678-9abc-123456789abc"),
                columns: new[] { "Address", "BVN", "CreditScore", "DateCreated", "DateOfBirth", "PhoneNumber" },
                values: new object[] { "13,Oshinowo street abule osho", "12345678901", 700, new DateTime(2025, 10, 8, 12, 24, 23, 714, DateTimeKind.Utc).AddTicks(9829), new DateTime(2025, 10, 8, 12, 24, 23, 714, DateTimeKind.Utc).AddTicks(9837), "08134570701" });
        }
    }
}
