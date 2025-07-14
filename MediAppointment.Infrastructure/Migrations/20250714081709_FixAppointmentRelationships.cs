using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAppointmentRelationships : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_RoomTimeSlot_TimeSlot_TimeSlotId",
                table: "RoomTimeSlot");

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId1",
                table: "RoomTimeSlot",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId1",
                table: "RoomTimeSlot",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomTimeSlot_DoctorId1",
                table: "RoomTimeSlot",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTimeSlot_RoomId1",
                table: "RoomTimeSlot",
                column: "RoomId1");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTimeSlot_Room_RoomId1",
                table: "RoomTimeSlot",
                column: "RoomId1",
                principalTable: "Room",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTimeSlot_TimeSlot_TimeSlotId",
                table: "RoomTimeSlot",
                column: "TimeSlotId",
                principalTable: "TimeSlot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTimeSlot_User_DoctorId1",
                table: "RoomTimeSlot",
                column: "DoctorId1",
                principalTable: "User",
                principalColumn: "Id");
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

            migrationBuilder.DropForeignKey(
                name: "FK_RoomTimeSlot_Room_RoomId1",
                table: "RoomTimeSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomTimeSlot_TimeSlot_TimeSlotId",
                table: "RoomTimeSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomTimeSlot_User_DoctorId1",
                table: "RoomTimeSlot");

            migrationBuilder.DropIndex(
                name: "IX_RoomTimeSlot_DoctorId1",
                table: "RoomTimeSlot");

            migrationBuilder.DropIndex(
                name: "IX_RoomTimeSlot_RoomId1",
                table: "RoomTimeSlot");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "RoomTimeSlot");

            migrationBuilder.DropColumn(
                name: "RoomId1",
                table: "RoomTimeSlot");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTimeSlot_TimeSlot_TimeSlotId",
                table: "RoomTimeSlot",
                column: "TimeSlotId",
                principalTable: "TimeSlot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
