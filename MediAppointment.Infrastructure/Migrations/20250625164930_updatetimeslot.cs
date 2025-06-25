using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatetimeslot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Shift",
                table: "TimeSlot",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Shift",
                table: "TimeSlot");
        }
    }
}
