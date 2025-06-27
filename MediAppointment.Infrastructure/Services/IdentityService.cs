using System.Security.Claims;
using MediAppointment.Application.Constants;
using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.Auth;
using MediAppointment.Application.DTOs.DoctorDTOs;
using MediAppointment.Application.DTOs.Pages;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Enums;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MediAppointment.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        #region Constructor and Dependency Injection
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
        #endregion

        #region ManagerDoctor
        // GET ALL
        public async Task<PagedResult<DoctorDto>> GetAllDoctorsAsync(string text = "", string department = "", int page = 1, int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var listDoctor = _dbContext.Set<User>().OfType<Doctor>().AsQueryable();
            listDoctor = listDoctor.Include(d => d.DoctorDepartments).ThenInclude(dd => dd.Department);

            if (!string.IsNullOrWhiteSpace(text))
            {
                text = text.Trim().ToLower();
                listDoctor = listDoctor.Where(d => d.FullName.ToLower().Contains(text) || d.Email.ToLower().Contains(text));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                department = department.Trim().ToLower();
                listDoctor = listDoctor.Where(d => d.DoctorDepartments.Any(dd => dd.Department.DepartmentName.ToLower().Contains(department)));
            }

            var totalCount = await listDoctor.CountAsync();

            // phân trang và thông tin all bác sĩ
            var doctors = await listDoctor
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Gender = d.Gender,
                    DateOfBirth = d.DateOfBirth,
                    Email = d.Email,
                    PhoneNumber = d.PhoneNumber,
                    Departments = d.DoctorDepartments.Select(dd => dd.Department.DepartmentName).ToList(),
                    Status = (int)d.Status
                })
                .ToListAsync();

            return new PagedResult<DoctorDto>
            {
                Items = doctors,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // CREATE
        public async Task<Guid> CreateDoctorAsync(DoctorCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.");
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new ArgumentException("Email is already taken.");

            string password = GenerateRandomPassword(8);

            var userIdentity = new UserIdentity
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber
            };
            var result = await _userManager.CreateAsync(userIdentity, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create UserIdentity: {string.Join("; ", result.Errors.Select(e => e.Description))}");

            var roleResult = await _userManager.AddToRoleAsync(userIdentity, UserRoles.Doctor);
            if (!roleResult.Succeeded)
                throw new Exception($"Failed to assign Doctor role: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Gender = dto.Gender ?? true,
                DateOfBirth = dto.DateOfBirth?.Date ?? DateTime.UtcNow.Date,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Status = Status.Active
            };
            await _unitOfWork.Repository<Doctor>().AddAsync(doctor);
            _dbContext.Entry(doctor).Property("UserIdentityId").CurrentValue = userIdentity.Id;

            if (dto.Departments != null && dto.Departments.Any())
            {
                var validDepartmentIds = await _dbContext.Departments
                    .Where(d => dto.Departments.Contains(d.Id))
                    .Select(d => d.Id)
                    .ToListAsync();

                if (validDepartmentIds.Count != dto.Departments.Count)
                {
                    var invalidIds = dto.Departments.Except(validDepartmentIds).ToList();
                    throw new ArgumentException($"Invalid department IDs: {string.Join(", ", invalidIds)}");
                }

                foreach (var deptId in validDepartmentIds)
                {
                    var doctorDepartment = new DoctorDepartment
                    {
                        DoctorId = doctor.Id,
                        DepartmentId = deptId
                    };
                    _dbContext.Set<DoctorDepartment>().Add(doctorDepartment);
                }
            }

            // 6. Send email with generated password (if applicable)
            //var subject = "Your MediAppointment Account Credentials";
            //var body = $@"
            //        <p>Hello {dto.FullName},</p>
            //        <p>Your MediAppointment doctor account has been created.</p>
            //        <p><strong>Email:</strong> {dto.Email}</p>
            //        <p><strong>Password:</strong> {password}</p>
            //        <p>Please log in and change your password immediately.</p>
            //        <p><a href=""https://your-app-url/login"">Log in here</a></p>
            //    ";
            //await _emailService.SendAsync(dto.Email, subject, body);

            await _dbContext.SaveChangesAsync();
            return doctor.Id;
        }

        // UPDATE
        public async Task ManagerUpdateDoctorAsync(ManagerDoctorUpdateDTO dto)
        {
            var doctor = await _dbContext.Set<User>().OfType<Doctor>().Include(d => d.DoctorDepartments).FirstOrDefaultAsync(d => EF.Property<Guid?>(d, "UserIdentityId") == dto.UserIdentityId)
                ?? throw new ArgumentException($"Doctor with UserIdentityId {dto.UserIdentityId} not found.");

            doctor.Status = dto.Status;

            var currentDepartmentIds = doctor.DoctorDepartments.Select(dd => dd.DepartmentId).ToList();
            var newDepartmentIds = dto.Departments ?? new List<Guid>();

            if (newDepartmentIds.Any())
            {
                var validDepartmentIds = await _dbContext.Departments
                    .Where(d => newDepartmentIds.Contains(d.Id))
                    .Select(d => d.Id)
                    .ToListAsync();

                if (validDepartmentIds.Count != newDepartmentIds.Count)
                {
                    var invalidIds = newDepartmentIds.Except(validDepartmentIds).ToList();
                    throw new ArgumentException($"Invalid department IDs: {string.Join(", ", invalidIds)}");
                }
            }

            var departmentsToRemove = doctor.DoctorDepartments.Where(dd => !newDepartmentIds.Contains(dd.DepartmentId)).ToList();
            foreach (var dept in departmentsToRemove)
            {
                _dbContext.Set<DoctorDepartment>().Remove(dept);
            }

            var departmentsToAdd = newDepartmentIds.Where(id => !currentDepartmentIds.Contains(id))
                .Select(id => new DoctorDepartment
                {
                    DoctorId = doctor.Id,
                    DepartmentId = id
                }).ToList();
            _dbContext.Set<DoctorDepartment>().AddRange(departmentsToAdd);

            await _dbContext.SaveChangesAsync();
        }

        // DELETE
        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            var doctor = await _dbContext.Set<User>().OfType<Doctor>().FirstOrDefaultAsync(d => d.Id == doctorId)
                ?? throw new ArgumentException($"Doctor with UserId {doctorId} not found.");

            doctor.Status = Domain.Enums.Status.Deleted;
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region Helper_method
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region DoctorUpdateProfile
        public async Task<DoctorDto> GetDoctorByIdAsync(Guid doctorId)
        {
            var doctor = await _dbContext.Set<User>().OfType<Doctor>().Include(d => d.DoctorDepartments).ThenInclude(dd => dd.Department).FirstOrDefaultAsync(d => d.Id == doctorId)
                ?? throw new ArgumentException($"Doctor with UserId {doctorId} not found.");

            return new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Gender = doctor.Gender,
                DateOfBirth = doctor.DateOfBirth,
                Email = doctor.Email,
                PhoneNumber = doctor.PhoneNumber,
                Departments = doctor.DoctorDepartments.Select(dd => dd.Department.DepartmentName).ToList(),
                Status = (int)doctor.Status
            };
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

            //    if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            //    {
            //        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            //            throw new Exception("Current password is required to change password.");
            //        if (!await _userManager.CheckPasswordAsync(userIdentity, dto.CurrentPassword))
            //            throw new Exception("Current password is incorrect.");
            //        if (dto.NewPassword != dto.ConfirmNewPassword)
            //            throw new Exception("New password and confirmation do not match.");
            //
            //        var passwordChangeResult = await _userManager.ChangePasswordAsync(userIdentity, dto.CurrentPassword, dto.NewPassword);
            //        if (!passwordChangeResult.Succeeded)
            //           throw new Exception(string.Join("; ", passwordChangeResult.Errors.Select(e => e.Description)));
            //        hasChanges = true;
            //    }

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
        #endregion

        #region Patient_CRUD
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
        #endregion

        #region Login
        public async Task<LoginResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new LoginResultDto { Success = false, ErrorMessage = "Tài khoản không tồn tại." };

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return new LoginResultDto { Success = false, ErrorMessage = "Sai mật khẩu." };

            // Tìm domain user (Doctor/Patient) theo UserIdentityId
            var userId = Guid.Empty;
            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => EF.Property<Guid?>(d, "UserIdentityId") == user.Id);
            if(doctor != null)
                userId = doctor.Id;

            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => EF.Property<Guid?>(p, "UserIdentityId") == user.Id);
            if (patient != null)
                userId = patient.Id;

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()!),
                new Claim(ClaimTypes.Email, dto.Email),
                new Claim(ClaimTypes.Name, dto.Email),
                new Claim("UserId", userId.ToString()!)
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
        #endregion

        #region Register
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
        #endregion

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

        #region Logout
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        #endregion

        #region ForgotPassword
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
        #endregion

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
