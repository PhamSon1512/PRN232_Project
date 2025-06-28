using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.DTOs.Payments
{
    public class PatientDetailResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public bool Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string CCCD { get; set; } = "";
        public string Address { get; set; } = "";
        public string BHYT { get; set; } = "";

        //public List<MedicalRecordDto> MedicalRecords { get; set; } = new();
    }

}
