using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updated_customer_balance_and_added_payment_history : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReservationId",
                table: "CustomerBalances",
                newName: "SourceId");

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "CustomerBalances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PaymentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBalances_CustomerId",
                table: "CustomerBalances",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBalances_CustomerId_SourceType",
                table: "CustomerBalances",
                columns: new[] { "CustomerId", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_CustomerId",
                table: "PaymentHistories",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_CustomerId_SourceType",
                table: "PaymentHistories",
                columns: new[] { "CustomerId", "SourceType" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistories_PaymentDate",
                table: "PaymentHistories",
                column: "PaymentDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentHistories");

            migrationBuilder.DropIndex(
                name: "IX_CustomerBalances_CustomerId",
                table: "CustomerBalances");

            migrationBuilder.DropIndex(
                name: "IX_CustomerBalances_CustomerId_SourceType",
                table: "CustomerBalances");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "CustomerBalances");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "CustomerBalances",
                newName: "ReservationId");
        }
    }
}
