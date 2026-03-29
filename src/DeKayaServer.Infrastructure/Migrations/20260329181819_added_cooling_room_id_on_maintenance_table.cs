using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class added_cooling_room_id_on_maintenance_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoolingRoomId",
                table: "CoolingRoomMaintenances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoolingRoomId",
                table: "CoolingRoomMaintenances");
        }
    }
}
