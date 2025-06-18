using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateToRoomTimeSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "RoomTimeSlot",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "RoomTimeSlot");
        }
    }
}
