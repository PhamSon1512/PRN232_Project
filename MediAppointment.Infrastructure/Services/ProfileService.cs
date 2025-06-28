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

        public async Task<Doctor> UpdateProfileAsync(Guid userId, DoctorUpdateDto dto)
        {
            return await _identityService.UpdateDoctorAsync(userId, dto);
        }
    }
}