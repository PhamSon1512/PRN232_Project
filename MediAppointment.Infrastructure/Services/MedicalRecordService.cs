using AutoMapper;
using MediAppointment.Application.DTOs.MedicalRecordDtos;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;


namespace MediAppointment.Infrastructure.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IGenericRepository<MedicalRecord> _medicalRecordRepo;
        protected readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MedicalRecordService(
            IGenericRepository<MedicalRecord> medicalRecordRepo,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _medicalRecordRepo = medicalRecordRepo;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Guid> CreateMedicalRecordAsync(CreateMedicalRecordDto dto)
        {
            // Kiểm tra bệnh nhân tồn tại
            var patientExists = await _context.Users.AnyAsync(u => u.Id == dto.PatientId);
            if (!patientExists)
                throw new ArgumentException("Patient not found");


            // Nếu có DoctorId thì kiểm tra luôn
            if (dto.DoctorId.HasValue)
            {
                var doctorExists = await _context.Users.AnyAsync(u => u.Id == dto.DoctorId.Value);
                if (!doctorExists)
                    throw new ArgumentException("Doctor not found");
            }

            // Ánh xạ và lưu
            var record = _mapper.Map<MedicalRecord>(dto);
            record.Id = Guid.NewGuid();
            record.LastUpdated = DateTime.UtcNow.ToString("yyyy-MM-dd");

            await _medicalRecordRepo.AddAsync(record);
            return record.Id;
        }
    }
}
