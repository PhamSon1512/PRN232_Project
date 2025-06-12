using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediAppointment.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public IdentityService(
            UserManager<UserIdentity> userManager,
            SignInManager<UserIdentity> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        // Doctor CRUD
        public async Task<Guid> CreateDoctorAsync(DoctorCreateDto dto)
        {
            // 1. Tạo UserIdentity
            var userIdentity = new UserIdentity
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber
            };
            var result = await _userManager.CreateAsync(userIdentity, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            // 2. Tạo Doctor, gán UserIdentityId (shadow property)
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };
            _dbContext.Doctors.Add(doctor);
            _dbContext.Entry(doctor).Property("UserIdentityId").CurrentValue = userIdentity.Id;
            await _dbContext.SaveChangesAsync();
            return doctor.Id;
        }

        public async Task UpdateDoctorAsync(DoctorUpdateDto dto)
        {
            // 1. Lấy Doctor và UserIdentityId
            var doctor = await _dbContext.Doctors.FindAsync(dto.Id)
                ?? throw new Exception("Doctor not found");
            var userIdentityId = _dbContext.Entry(doctor).Property<Guid?>("UserIdentityId").CurrentValue;
            if (userIdentityId == null)
                throw new Exception("UserIdentity not found");

            // 2. Cập nhật Doctor
            doctor.FullName = dto.FullName;
            doctor.Gender = dto.Gender;
            doctor.DateOfBirth = dto.DateOfBirth;
            doctor.Email = dto.Email;
            doctor.PhoneNumber = dto.PhoneNumber;

            // 3. Cập nhật UserIdentity
            var userIdentity = await _userManager.FindByIdAsync(userIdentityId.ToString()!);
            if (userIdentity != null)
            {
                userIdentity.FullName = dto.FullName;
                userIdentity.Email = dto.Email;
                userIdentity.UserName = dto.Email;
                userIdentity.PhoneNumber = dto.PhoneNumber;
                await _userManager.UpdateAsync(userIdentity);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            var doctor = await _dbContext.Doctors.FindAsync(doctorId)
                ?? throw new Exception("Doctor not found");
            _dbContext.Doctors.Remove(doctor);
            await _dbContext.SaveChangesAsync();
            // UserIdentity sẽ bị xóa cascade nếu cấu hình đúng
        }

        public async Task<DoctorUpdateDto?> GetDoctorByIdAsync(Guid doctorId)
        {
            var doctor = await _dbContext.Doctors.FindAsync(doctorId);
            if (doctor == null) return null;
            return new DoctorUpdateDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber
            };
        }

        // Patient CRUD
        public async Task<Guid> CreatePatientAsync(PatientCreateDto dto)
        {
            // 1. Tạo UserIdentity
            var userIdentity = new UserIdentity
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber
            };
            var result = await _userManager.CreateAsync(userIdentity, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

            // 2. Tạo Patient, gán UserIdentityId (shadow property)
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };
            _dbContext.Patients.Add(patient);
            _dbContext.Entry(patient).Property("UserIdentityId").CurrentValue = userIdentity.Id;
            await _dbContext.SaveChangesAsync();
            return patient.Id;
        }

        public async Task UpdatePatientAsync(PatientUpdateDto dto)
        {
            var patient = await _dbContext.Patients.FindAsync(dto.Id)
                ?? throw new Exception("Patient not found");
            var userIdentityId = _dbContext.Entry(patient).Property<Guid?>("UserIdentityId").CurrentValue;
            if (userIdentityId == null)
                throw new Exception("UserIdentity not found");

            patient.FullName = dto.FullName;
            patient.Gender = dto.Gender;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Email = dto.Email;
            patient.PhoneNumber = dto.PhoneNumber;

            var userIdentity = await _userManager.FindByIdAsync(userIdentityId.ToString()!);
            if (userIdentity != null)
            {
                userIdentity.FullName = dto.FullName;
                userIdentity.Email = dto.Email;
                userIdentity.UserName = dto.Email;
                userIdentity.PhoneNumber = dto.PhoneNumber;
                await _userManager.UpdateAsync(userIdentity);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeletePatientAsync(Guid patientId)
        {
            var patient = await _dbContext.Patients.FindAsync(patientId)
                ?? throw new Exception("Patient not found");
            _dbContext.Patients.Remove(patient);
            await _dbContext.SaveChangesAsync();
            // UserIdentity sẽ bị xóa cascade nếu cấu hình đúng
        }

        public async Task<PatientUpdateDto?> GetPatientByIdAsync(Guid patientId)
        {
            var patient = await _dbContext.Patients.FindAsync(patientId);
            if (patient == null) return null;
            return new PatientUpdateDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber
            };
        }

        // Login
        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new LoginResultDto { Success = false, ErrorMessage = "Tài khoản không tồn tại." };

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return new LoginResultDto { Success = false, ErrorMessage = "Sai mật khẩu." };

            // Tìm domain user (Doctor/Patient) theo UserIdentityId
            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => EF.Property<Guid?>(d, "UserIdentityId") == user.Id);
            if (doctor != null)
                return new LoginResultDto { Success = true, UserId = doctor.Id, Role = "Doctor" };

            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => EF.Property<Guid?>(p, "UserIdentityId") == user.Id);
            if (patient != null)
                return new LoginResultDto { Success = true, UserId = patient.Id, Role = "Patient" };

            return new LoginResultDto { Success = false, ErrorMessage = "Không tìm thấy thông tin người dùng." };
        }
    }
}
