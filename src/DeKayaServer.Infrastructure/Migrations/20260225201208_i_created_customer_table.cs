using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class i_created_customer_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    LastName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    FullName_Value = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Address_District = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Address_FullAddress = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Contact_PhoneNumber = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Contact_PhoneNumber2 = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Contact_Email = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
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
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
