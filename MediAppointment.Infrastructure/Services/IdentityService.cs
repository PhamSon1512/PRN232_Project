using System.Security.Claims;
using MediAppointment.Application.Constants;
using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MediAppointment.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailSender<UserIdentity> _emailSender;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public IdentityService(
            UserManager<UserIdentity> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            SignInManager<UserIdentity> signInManager,
            ApplicationDbContext dbContext,
            IEmailSender<UserIdentity> emailSender,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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
            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
            _dbContext.Entry(doctor).Property("UserIdentityId").CurrentValue = userIdentity.Id;
            return doctor.Id;
        }

        public async Task UpdateDoctorAsync(DoctorUpdateDto dto)
        {
            // 1. Lấy Doctor và UserIdentityId
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(dto.Id)
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

            // Nếu repository có UpdateAsync thì gọi:
            await _unitOfWork.Repository<Doctor>().UpdateAsync(doctor);

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

            // Lưu thay đổi qua UnitOfWork
            await _unitOfWork.Save(CancellationToken.None);
        }


        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            // 1. Lấy doctor qua repository
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId)
                ?? throw new Exception("Doctor not found");

            // 2. Xóa doctor qua repository
            await _unitOfWork.Repository<Doctor>().DeleteAsync(doctor);

            // 3. Lưu thay đổi qua UnitOfWork
            await _unitOfWork.Save(CancellationToken.None);
        }


        public async Task<DoctorUpdateDto?> GetDoctorByIdAsync(Guid doctorId)
        {
            // Lấy doctor qua repository
            var doctor = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);
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
            //var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => EF.Property<Guid?>(d, "UserIdentityId") == user.Id);
            //if (doctor != null)
            //    return new LoginResultDto { Success = true, UserId = user.Id, Role = "Doctor" };

            //var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => EF.Property<Guid?>(p, "UserIdentityId") == user.Id);
            //if (patient != null)
            //    return new LoginResultDto { Success = true, UserId = patient.Id, Role = "Patient" };

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()!),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Name, dto.Email),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"]));
            await _userManager.UpdateAsync(user);
            return new LoginResultDto
            {
                Success = true,
                ErrorMessage = "Đăng nhập thành công",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
            };
        }
        public async Task<LoginResultDto> RegisterAsync(RegisterDto dto)
        {
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
            var addRoleResult = await _userManager.AddToRolesAsync(userIdentity, ["Patient"]);
            if (!addRoleResult.Succeeded)
                return new LoginResultDto { Success = false, ErrorMessage = string.Join("; ", addRoleResult.Errors.Select(e => e.Description)) };

            Guid? patientId = null;

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


            // 4. Gửi email xác minh
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(userIdentity);
            var encodedToken = Uri.EscapeDataString(token);
            var confirmLink = $"https://localhost:7230/api/auth/confirm-email?email={Uri.EscapeDataString(dto.Email)}&token={encodedToken}";

            // Gửi mail
            await _emailSender.SendConfirmationLinkAsync(userIdentity, dto.Email, confirmLink);
            await _dbContext.SaveChangesAsync();

            // 4. Trả về kết quả
            return new LoginResultDto
            {
                Success = true,
                UserId = patientId,
                ErrorMessage = "Đăng ký thành công. Vui lòng xác minh email"
            };
        }

        public async Task<LoginResultDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            var userEmail = principal.Identity.Name;
            var user = await _userManager.FindByNameAsync(userEmail);

            if (user == null || user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            var claims = principal.Claims.ToList();
            var newAccessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new LoginResultDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
        }
        //Confirm Email
        public async Task<LoginResultDto> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Không tìm thấy người dùng."
                };
            }

            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = $"Xác minh thất bại: {errors}"
                };
            }

            return new LoginResultDto
            {
                Success = true,
                UserId = user.Id
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
                return false;

            // Sinh token đặt lại mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Tạo link đặt lại mật khẩu 
            var encodedToken = Uri.EscapeDataString(token);

            var resetLink = $"https://localhost:7230/api/auth/reset-password?email={Uri.EscapeDataString(dto.Email)}&token={encodedToken}";
            //var resetLink = $"{(token)}";

            await _emailSender.SendPasswordResetLinkAsync(user, dto.Email, resetLink);

            return true;
        }

        public async Task<LoginResultDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Tài khoản không tồn tại."
                };
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = $"Đặt lại mật khẩu thất bại: {errors}"
                };
            }

            return new LoginResultDto
            {
                Success = true,
                UserId = user.Id,
                ErrorMessage = null,
                Role = null,
                AccessToken = null
            };
        }

        //// Tạo role mới
        //public async Task<bool> CreateRoleAsync(string roleName)
        //{
        //    var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        //    if (!result.Succeeded)
        //    {
        //        throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        //    }
        //    return result.Succeeded;
        //}

        //// Xóa role theo Id
        //public async Task<bool> DeleteRoleAsync(string roleId)
        //{
        //    var role = await _roleManager.FindByIdAsync(roleId);
        //    if (role == null)
        //        throw new Exception("Role not found");
        //    var result = await _roleManager.DeleteAsync(role);
        //    if (!result.Succeeded)
        //        throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        //    return result.Succeeded;
        //}

        //// Lấy danh sách tất cả role
        //public async Task<List<(Guid id, string roleName)>> GetRolesAsync()
        //{
        //    var roles = await _roleManager.Roles.ToListAsync();
        //    return roles.Select(r => (r.Id, r.Name!)).ToList(); 
        //}


        //// Lấy role theo Id
        //public async Task<(Guid id, string roleName)> GetRoleByIdAsync(string id)
        //{
        //    var role = await _roleManager.FindByIdAsync(id);
        //    if (role == null)
        //        throw new Exception("Role not found");
        //    return (role.Id, role.Name!);
        //}

        //// Cập nhật tên role
        //public async Task<bool> UpdateRole(Guid id, string roleName)
        //{
        //    var role = await _roleManager.FindByIdAsync(id.ToString());
        //    if (role == null)
        //        throw new Exception("Role not found");
        //    role.Name = roleName;
        //    var result = await _roleManager.UpdateAsync(role);
        //    if (!result.Succeeded)
        //        throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        //    return result.Succeeded;
        //}

        //// Kiểm tra user có thuộc role không
        //public async Task<bool> IsInRoleAsync(string userId, string role)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //        throw new Exception("User not found");
        //    return await _userManager.IsInRoleAsync(user, role);
        //}

        //// Lấy danh sách role của user
        //public async Task<List<string>> GetUserRolesAsync(string userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //        throw new Exception("User not found");
        //    var roles = await _userManager.GetRolesAsync(user);
        //    return roles.ToList();
        //}

        //// Gán user vào nhiều role
        //public async Task<bool> AssignUserToRole(string userName, IList<string> roles)
        //{
        //    var user = await _userManager.FindByNameAsync(userName);
        //    if (user == null)
        //        throw new Exception("User not found");
        //    var result = await _userManager.AddToRolesAsync(user, roles);
        //    if (!result.Succeeded)
        //        throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
        //    return result.Succeeded;
        //}

        //// Cập nhật lại toàn bộ role của user
        //public async Task<bool> UpdateUsersRole(string userName, IList<string> usersRole)
        //{
        //    var user = await _userManager.FindByNameAsync(userName);
        //    if (user == null)
        //        throw new Exception("User not found");
        //    var currentRoles = await _userManager.GetRolesAsync(user);
        //    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        //    if (!removeResult.Succeeded)
        //        throw new Exception(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        //    var addResult = await _userManager.AddToRolesAsync(user, usersRole);
        //    if (!addResult.Succeeded)
        //        throw new Exception(string.Join("; ", addResult.Errors.Select(e => e.Description)));
        //    return addResult.Succeeded;
        //}

        //// Kiểm tra user tồn tại và đã xác thực email chưa
        //public async Task<(bool isUserExists, bool isConfirmed)> CheckUserExistsWithEmailConfirmedAsync(string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //        return (false, false);
        //    return (true, user.EmailConfirmed);
        //}

        //// Sinh token xác thực email
        //public async Task<string> GenerateEmailConfirmationTokenAsync(string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //        throw new Exception("User not found");
        //    return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //}

    }
}