using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyUserIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspNetUsers_UserIdentityId",
                table: "User");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspNetUsers_UserIdentityId",
                table: "User",
                column: "UserIdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_AspNetUsers_UserIdentityId",
                table: "User");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AspNetUsers_UserIdentityId",
                table: "User",
                column: "UserIdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
