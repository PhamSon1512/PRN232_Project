using MediAppointment.Application.DTOs.MedicalRecordDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.PatientDTOs
{
    public class PatientWithRecordsResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BHYT { get; set; } = string.Empty;

        public List<MedicalRecordViewDto> MedicalRecords { get; set; } = new();
    }
}
