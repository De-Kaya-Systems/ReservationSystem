using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changed_customer_balance_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType_Value",
                table: "CustomerBalances");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentTypeId",
                table: "CustomerBalances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "CustomerBalances");

            migrationBuilder.AddColumn<string>(
                name: "PaymentType_Value",
                table: "CustomerBalances",
                type: "nvarchar(MAX)",
                nullable: true);
        }
    }
}
