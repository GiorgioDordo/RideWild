using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideWild.Migrations
{
    /// <inheritdoc />
    public partial class UniqueAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
               name: "IX_CustomerData_EmailAddress",
               table: "CustomerData",
               column: "EmailAddress",
               unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
               name: "IX_CustomerData_EmailAddress",
               table: "CustomerData");
        }
    }
}
