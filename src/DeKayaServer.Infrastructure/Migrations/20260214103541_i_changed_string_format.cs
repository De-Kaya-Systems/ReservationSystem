using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeKayaServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class i_changed_string_format : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Email_Value",
                table: "Users",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Name_Value",
                table: "Roles",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Permission",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Token_Value",
                table: "LoginTokens",
                type: "nvarchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(MAX)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName_Value",
                table: "Users",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName_Value",
                table: "Users",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName_Value",
                table: "Users",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName_Value",
                table: "Users",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Email_Value",
                table: "Users",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Name_Value",
                table: "Roles",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Permission",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");

            migrationBuilder.AlterColumn<string>(
                name: "Token_Value",
                table: "LoginTokens",
                type: "varchar(MAX)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(MAX)");
        }
    }
}
