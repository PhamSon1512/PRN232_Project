using MediAppointment.Application.Constants;
using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MediAppointment.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _dbContext;

        public IdentityService(
            UserManager<UserIdentity> userManager,
            SignInManager<UserIdentity> signInManager,
            ApplicationDbContext dbContext,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _emailService = emailService;
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

        public async Task UpdateDoctorAsync(Guid userIdentityId, DoctorUpdateDto dto)
        {
            // AspNetUsers  
            var userIdentity = await _userManager.FindByIdAsync(userIdentityId.ToString())
                ?? throw new Exception("UserIdentity not found");

            // Retrieve Doctor directly using TPH  
            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => EF.Property<Guid?>(d, "UserIdentityId") == userIdentityId)
                ?? throw new Exception("Doctor not found");

            // Compare UserIdentityId (shadow property) with AspNetUser.Id  
            var userIdentityIdShadow = _dbContext.Entry(doctor).Property<Guid?>("UserIdentityId").CurrentValue;
            if (userIdentityIdShadow != userIdentityId)
                throw new Exception("Mismatch between User.UserIdentityId and AspNetUser.Id");

            bool hasChanges = false;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != doctor.PhoneNumber)
            {
                doctor.PhoneNumber = dto.PhoneNumber;
                userIdentity.PhoneNumber = dto.PhoneNumber;
                hasChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                    throw new Exception("Current password is required to change password.");
                if (!await _userManager.CheckPasswordAsync(userIdentity, dto.CurrentPassword))
                    throw new Exception("Current password is incorrect.");
                if (dto.NewPassword != dto.ConfirmNewPassword)
                    throw new Exception("New password and confirmation do not match.");

                var passwordChangeResult = await _userManager.ChangePasswordAsync(userIdentity, dto.CurrentPassword, dto.NewPassword);
                if (!passwordChangeResult.Succeeded)
                    throw new Exception(string.Join("; ", passwordChangeResult.Errors.Select(e => e.Description)));
                hasChanges = true;
            }

            if (hasChanges)
            {
                // update AspNetUser  
                var updateIdentityResult = await _userManager.UpdateAsync(userIdentity);
                if (!updateIdentityResult.Succeeded)
                    throw new Exception(string.Join("; ", updateIdentityResult.Errors.Select(e => e.Description)));

                //update Users  
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            var doctor = await _dbContext.Doctors.FindAsync(doctorId)
                ?? throw new Exception("Doctor not found");
            _dbContext.Doctors.Remove(doctor);
            await _dbContext.SaveChangesAsync();
            // UserIdentity sẽ bị xóa cascade nếu cấu hình đúng
        }

        //public async Task<DoctorUpdateDto?> GetDoctorByIdAsync(Guid doctorId)
        //{
        //    var doctor = await _dbContext.Doctors.FindAsync(doctorId);
        //    if (doctor == null) return null;
        //    return new DoctorUpdateDto
        //    {
        //        Id = doctor.Id,
        //        FullName = doctor.FullName,
        //        Gender = doctor.Gender,
        //        DateOfBirth = doctor.DateOfBirth,
        //        Email = doctor.Email,
        //        PhoneNumber = doctor.PhoneNumber
        //    };
        //}

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
                return new LoginResultDto { Success = true, UserId = user.Id, Role = "Doctor" };

            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => EF.Property<Guid?>(p, "UserIdentityId") == user.Id);
            if (patient != null)
                return new LoginResultDto { Success = true, UserId = patient.Id, Role = "Patient" };

            return new LoginResultDto { Success = false, ErrorMessage = "Không tìm thấy thông tin người dùng." };
        }
        public async Task<LoginResultDto> RegisterAsync(RegisterDto dto)
        {
            if (dto.Roles == null || !dto.Roles.Any())
                return new LoginResultDto { Success = false, ErrorMessage = "At least one role is required." };

            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return new LoginResultDto { Success = false, ErrorMessage = "Email already taken." };

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
                return new LoginResultDto { Success = false, ErrorMessage = string.Join("; ", result.Errors.Select(e => e.Description)) };

            // 2. Gán nhiều role cho user
            var addRoleResult = await _userManager.AddToRolesAsync(userIdentity, dto.Roles);
            if (!addRoleResult.Succeeded)
                return new LoginResultDto { Success = false, ErrorMessage = string.Join("; ", addRoleResult.Errors.Select(e => e.Description)) };

            Guid? doctorId = null;
            Guid? patientId = null;

            // 3. Tạo entity domain tương ứng và gán UserIdentityId
            if (dto.Roles.Contains(UserRoles.Doctor, StringComparer.OrdinalIgnoreCase))
            {
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
                doctorId = doctor.Id;
            }
            if (dto.Roles.Contains(UserRoles.Patient, StringComparer.OrdinalIgnoreCase))
            {
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
                patientId = patient.Id;
            }

            await _dbContext.SaveChangesAsync();

            // 4. Trả về kết quả
            return new LoginResultDto
            {
                Success = true,
                UserId = doctorId ?? patientId, // Ưu tiên DoctorId nếu có, hoặc PatientId
                Role = string.Join(",", dto.Roles)
            };
        }

        //Logout
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        //Forgot
        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return false; // Không tiết lộ thông tin user tồn tại

            // Sinh token đặt lại mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Tạo link đặt lại mật khẩu (ví dụ: Razor Page hoặc API endpoint)
            var resetLink = $"https://localhost:7230/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={Uri.EscapeDataString(token)}";

            // Soạn nội dung email
            var subject = "Đặt lại mật khẩu MediAppointment";
            var body = $@"
        <p>Xin chào,</p>
        <p>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản MediAppointment.</p>
        <p>Vui lòng nhấn vào liên kết sau để đặt lại mật khẩu:</p>
        <p><a href=""{resetLink}"">Đặt lại mật khẩu</a></p>
        <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
    ";

            await _emailService.SendAsync(dto.Email, subject, body);
            return true;
        }
    }
}
