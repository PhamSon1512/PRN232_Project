using MediAppointment.Application.DTOs;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace MediAppointment.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly UserManager<UserIdentity> _userManager;
        private readonly IEmailService _emailService;
        private readonly IIdentityService _identityService;

        public ProfileService(IProfileRepository profileRepository, UserManager<UserIdentity> userManager, IEmailService emailService, IIdentityService identityService)
        {
            _profileRepository = profileRepository;
            _userManager = userManager;
            _emailService = emailService;
            _identityService = identityService;
        }

        public async Task<Doctor?> GetProfileByIdAsync(Guid userIdentityId)
        {
            return await _profileRepository.GetProfileByIdAsync(userIdentityId);
        }

        public async Task<Doctor> UpdateProfileAsync(DoctorUpdateDto dto)
        {
            await _identityService.UpdateDoctorAsync(dto.UserIdentityId, dto);
            var doctor = await _profileRepository.GetProfileByIdAsync(dto.UserIdentityId);
            if (doctor == null) throw new Exception("Doctor not found after update");
            return doctor;
        }
    }
}


    //private string GenerateRandomPassword(int length)
    //{
    //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    //    var random = new Random();
    //    return new string(Enumerable.Repeat(chars, length)
    //        .Select(s => s[random.Next(s.Length)]).ToArray());
    //}