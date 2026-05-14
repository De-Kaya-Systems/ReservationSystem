using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_pay_process_for_reservation : Migration
    {
        /// <inheritdoc />
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CoolingRoomBaseDailyPrice_Value",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m );

            migrationBuilder.Sql( """
        UPDATE Reservations
        SET CoolingRoomBaseDailyPrice_Value = CoolingRoomDailyPrice_Value
        WHERE CoolingRoomBaseDailyPrice_Value = 0
    """ );

            migrationBuilder.AddColumn<string>(
                name: "PriceOverrideNote_Value",
                table: "Reservations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true );

            migrationBuilder.AddColumn<string>(
                name: "PriceOverrideReason_Value",
                table: "Reservations",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true );
        }

        /// <inheritdoc />
        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "CoolingRoomBaseDailyPrice_Value",
                table: "Reservations" );

            migrationBuilder.DropColumn(
                name: "PriceOverrideNote_Value",
                table: "Reservations" );

            migrationBuilder.DropColumn(
                name: "PriceOverrideReason_Value",
                table: "Reservations" );
        }
    }
}
