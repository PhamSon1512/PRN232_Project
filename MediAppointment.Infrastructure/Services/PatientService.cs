using AutoMapper;
using MediAppointment.Application.DTOs.MedicalRecordDtos;
using MediAppointment.Application.DTOs.PatientDTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
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
        private readonly IGenericRepository<Patient> _patientRepository;
        private readonly IGenericRepository<MedicalRecord> _medicalRecordRepository;
        private readonly IMapper _mapper;

        public PatientService(
            IGenericRepository<Patient> patientRepository,
            IGenericRepository<MedicalRecord> medicalRecordRepository,
            IMapper mapper)
        {
            _patientRepository = patientRepository;
            _medicalRecordRepository = medicalRecordRepository;
            _mapper = mapper;
        }

        public async Task<PatientWithRecordsResponse?> GetPatientWithRecordsAsync(Guid patientId)
        {
            var patient = await _patientRepository.Entities
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return null;

            var medicalRecords = await _medicalRecordRepository.Entities
                .Where(m => m.PatientId == patientId)
                .ToListAsync();

            var response = new PatientWithRecordsResponse
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                CCCD = patient.CCCD,
                Address = patient.Address,
                BHYT = patient.BHYT,
                MedicalRecords = medicalRecords
                    .Select(record => _mapper.Map<MedicalRecordViewDto>(record))
                    .ToList()
            };

            return response;
        }
    
    }
}
