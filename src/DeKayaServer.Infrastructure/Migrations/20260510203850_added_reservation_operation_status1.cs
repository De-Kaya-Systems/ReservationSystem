using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_reservation_operation_status1 : Migration
    {
        /// <inheritdoc />
        protected override void Up( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt_Value",
                table: "Reservations",
                type: "datetime2",
                nullable: true );

            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt_Value",
                table: "Reservations",
                type: "datetime2",
                nullable: true );

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 1 );
        }

        /// <inheritdoc />
        protected override void Down( MigrationBuilder migrationBuilder )
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAt_Value",
                table: "Reservations" );

            migrationBuilder.DropColumn(
                name: "PickedUpAt_Value",
                table: "Reservations" );

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reservations" );
        }
    }
}
