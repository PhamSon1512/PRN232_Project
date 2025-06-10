using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class Patient : User
    {
        public string? CCCD { get; set; } // 12 digits
        public string? Address { get; set; } // Address of the patient
        public string? BHYT { get; set; }
        public int Age {
            get
            {
                var age = DateTime.Today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;
                return age;
            }
        } 
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<MedicalRecord>? MedicalRecords { get; set; }
        public ICollection<Notification>? Notifications { get; set; }

    }
}
