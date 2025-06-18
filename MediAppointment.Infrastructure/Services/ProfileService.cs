using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Identity;
using MediAppointment.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;

namespace MediAppointment.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<UserIdentity> _userManager;
        private readonly IEmailService _emailService;

        public ProfileService(IProfileRepository profileRepository, UserManager<UserIdentity> userManager, IEmailService emailService)
        {
            _profileRepository = profileRepository;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId)
        {
            return await _profileRepository.GetProfileByIdAsync(userIdentityId);
        }

        //public async Task<Doctor> CreateProfileAsync(DoctorCreateDto dto)
        //{
        //    var plainPassword = GenerateRandomPassword(8);

        //    // create user identity  
        //    var userIdentity = new UserIdentity
        //    {
        //        UserName = dto.Email,
        //        Email = dto.Email,
        //        FullName = dto.FullName,
        //        PhoneNumber = dto.PhoneNumber,
        //        EmailConfirmed = true
        //    };
        //    var result = await _userManager.CreateAsync(userIdentity, plainPassword);
        //    if (!result.Succeeded) throw new Exception("Tạo tài khoản đăng nhập thất bại");

        //    // create new Doctor map with UserIdentity  
        //    var doctor = new Doctor
        //    {
        //        Id = Guid.NewGuid(),
        //        FullName = dto.FullName,
        //        Gender = dto.Gender,
        //        DateOfBirth = dto.DateOfBirth,
        //        Email = dto.Email,
        //        PhoneNumber = dto.PhoneNumber,
        //    };
        //    await _profileRepository.AddAsync(doctor);
        //    await _profileRepository.SaveChangeAsync();

        //    // send email with random password  
        //    var subject = "Tài khoản bác sĩ MediAppointment";
        //    var body = $@"  
        //       <p>Xin chào <b>{doctor.FullName}</b>,</p>  
        //       <p>Tài khoản của bạn đã được tạo trên hệ thống MediAppointment.</p>  
        //       <p><b>Mật khẩu đăng nhập của bạn là: <span style='color:blue'>{plainPassword}</span></b></p>  
        //       <p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu.</p>  
        //       <p>Trân trọng,<br/>MediAppointment Team</p>  
        //   ";
        //    await _emailService.SendAsync(doctor.Email, subject, body);
        //    return doctor;
        //}

        //public async Task UpdateProfileAsync(Guid id, DoctorUpdateDto dto)
        //{
        //    var doctor = await _profileRepository.GetProfileByIdAsync(id);
        //    if (doctor == null) throw new Exception("Doctor not found");

        //    // Lấy UserIdentity theo email
        //    var userIdentity = await _userManager.FindByEmailAsync(doctor.Email);
        //    if (userIdentity == null) throw new Exception("UserIdentity not found");

        //    // Cập nhật thông tin profile
        //    doctor.FullName = dto.FullName;
        //    doctor.Gender = dto.Gender;
        //    doctor.DateOfBirth = dto.DateOfBirth;
        //    doctor.Email = dto.Email;
        //    doctor.PhoneNumber = dto.PhoneNumber;

        //    userIdentity.FullName = dto.FullName;
        //    userIdentity.Email = dto.Email;
        //    userIdentity.UserName = dto.Email;
        //    userIdentity.PhoneNumber = dto.PhoneNumber;

        //    if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        //    {
        //        var checkCurrent = await _userManager.CheckPasswordAsync(userIdentity, dto.CurrentPassword);
        //        if (!checkCurrent) throw new Exception("Mật khẩu hiện tại không đúng.");

        //        if (dto.NewPassword != dto.ConfirmNewPassword)
        //            throw new Exception("Mật khẩu xác nhận không khớp.");

        //        var result = await _userManager.ChangePasswordAsync(userIdentity, dto.CurrentPassword, dto.NewPassword);
        //        if (!result.Succeeded) throw new Exception("Cập nhật mật khẩu thất bại: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        //    }

        //    await _userManager.UpdateAsync(userIdentity);
        //    await _profileRepository.UpdateAsync(doctor);
        //    await _profileRepository.SaveChangeAsync();
        //}


        //private string GenerateRandomPassword(int length)
        //{
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //    var random = new Random();
        //    return new string(Enumerable.Repeat(chars, length)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());
        //}
    }
}