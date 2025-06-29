using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediAppointment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalRecordDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Allergies",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentVisited",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Medications",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAppointmentDate",
                table: "MedicalRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symptoms",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TreatmentPlan",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitalSigns",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Allergies",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "DepartmentVisited",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Medications",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "NextAppointmentDate",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "Symptoms",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "TreatmentPlan",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "VitalSigns",
                table: "MedicalRecords");
        }
    }
}
