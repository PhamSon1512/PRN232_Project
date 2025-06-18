using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAppointment01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeEnd",
                table: "TimeSlot",
                newName: "Duration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "TimeSlot",
                newName: "TimeEnd");
        }
    }
}
