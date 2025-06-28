using MediAppointment.Application.DTOs.MedicalRecordDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<Guid> CreateMedicalRecordAsync(CreateMedicalRecordDto dto);
    }
}
