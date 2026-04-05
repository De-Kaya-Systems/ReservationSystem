using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_reservation_table_and_customer_balance_table_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReservationId",
                table: "CustomerBalances",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryLocation_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    DeliveryDate_Value = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliveryTime_Value = table.Column<TimeOnly>(type: "time", nullable: false),
                    PickUpDate_Value = table.Column<DateOnly>(type: "date", nullable: false),
                    PickUpTime_Value = table.Column<TimeOnly>(type: "time", nullable: false),
                    TotalDay_Value = table.Column<int>(type: "int", nullable: false),
                    CoolingRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoolingRoomDailyPrice_Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationTotalAmount_Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    PaidAtReservation_Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CoolingRoomId",
                table: "Reservations",
                column: "CoolingRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "CustomerBalances");
        }
    }
}
