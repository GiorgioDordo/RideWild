using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideWild.Migrations
{
    /// <inheritdoc />
    public partial class AddLastPasswordChangeToCustomerData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMfaEnabled",
                table: "CustomerData",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChange",
                table: "CustomerData",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MfaCode",
                table: "CustomerData",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MfaCodeExpiresAt",
                table: "CustomerData",
                type: "datetime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMfaEnabled",
                table: "CustomerData");

            migrationBuilder.DropColumn(
                name: "LastPasswordChange",
                table: "CustomerData");

            migrationBuilder.DropColumn(
                name: "MfaCode",
                table: "CustomerData");

            migrationBuilder.DropColumn(
                name: "MfaCodeExpiresAt",
                table: "CustomerData");
        }
    }
}
