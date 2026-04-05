using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_customer_balance_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount_Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingAmount_Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount_Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    BalanceStatus_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    PaymentType_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    LastPaymentAt_Value = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_CustomerBalances", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerBalances");
        }
    }
}
