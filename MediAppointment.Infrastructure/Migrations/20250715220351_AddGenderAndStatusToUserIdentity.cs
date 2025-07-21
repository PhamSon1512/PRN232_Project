using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenderAndStatusToUserIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_RoomTimeSlot_RoomTimeSlotId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomTimeSlot_Room_RoomId",
                table: "RoomTimeSlot");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Gender",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_RoomTimeSlot_RoomTimeSlotId",
                table: "Appointments",
                column: "RoomTimeSlotId",
                principalTable: "RoomTimeSlot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTimeSlot_Room_RoomId",
                table: "RoomTimeSlot",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_RoomTimeSlot_RoomTimeSlotId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomTimeSlot_Room_RoomId",
                table: "RoomTimeSlot");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_RoomTimeSlot_RoomTimeSlotId",
                table: "Appointments",
                column: "RoomTimeSlotId",
                principalTable: "RoomTimeSlot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTimeSlot_Room_RoomId",
                table: "RoomTimeSlot",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
