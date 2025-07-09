using AutoMapper;
using MediAppointment.Application.DTOs.MedicalRecordDtos;
using MediAppointment.Application.DTOs.PatientDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Infrastructure.Services
{
    internal class PatientService: IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PatientService(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PatientWithRecordsResponse?> GetPatientWithRecordsAsync(Guid patientIdentityId)
        {

            var patientID = await _context.Patients.FindAsync(patientIdentityId);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserIdentityId == patientID.UserIdentityId);

            if (patient == null)
                return null;

            var medicalRecords = await _context.MedicalRecords
                .Where(mr => mr.PatientId == patientID.UserIdentityId)
                .ToListAsync();

            return new PatientWithRecordsResponse
            {
                Id = patientIdentityId,
                FullName = patient.FullName,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                CCCD = patient.CCCD ?? "",
                Address = patient.Address ?? "",
                BHYT = patient.BHYT ?? "",
                MedicalRecords = medicalRecords
                    .Select(record => _mapper.Map<MedicalRecordViewDto>(record))
                    .ToList()
            };
        }

    }
}
