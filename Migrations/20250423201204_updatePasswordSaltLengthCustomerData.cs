using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideWild.Migrations
{
    /// <inheritdoc />
    public partial class updatePasswordSaltLengthCustomerData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
               name: "PasswordSalt",
               table: "CustomerData",
               type: "nvarchar(60)",
               maxLength: 60,
               nullable: false,
               oldClrType: typeof(string),
               oldType: "nvarchar(10)",
               oldMaxLength: 10
           );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
               name: "PasswordSalt",
               table: "CustomerData",
               type: "nvarchar(10)",
               maxLength: 10,
               nullable: false,
               oldClrType: typeof(string),
               oldType: "nvarchar(60)",
               oldMaxLength: 60
           );
        }
    }
}
