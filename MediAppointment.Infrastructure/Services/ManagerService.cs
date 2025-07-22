using MediAppointment.Application.DTOs;
using MediAppointment.Application.DTOs.DoctorDTOs;
using MediAppointment.Application.DTOs.Pages;
using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities.Abstractions;
using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediAppointment.Domain.Interfaces;
using MediAppointment.Application.DTOs.ManagerDTOs;

namespace MediAppointment.Infrastructure.Services
{
    public class ManagerService : IManagerService
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly IEmailService _emailService;
        private readonly IIdentityService _identityService;
        private readonly IProfileRepository _profileRepository;

        public ManagerService(UserManager<UserIdentity> userManager, IEmailService emailService, IIdentityService identityService, IProfileRepository profileRepository)
        {
            _userManager = userManager;
            _emailService = emailService;
            _identityService = identityService;
            _profileRepository = profileRepository;
        }

        public async Task<PagedResult<DoctorDto>> GetAllDoctorsAsync(string text = "", /*string department = "",*/ int page = 1, int pageSize = 5)
        {
            return await _identityService.GetAllDoctorsAsync(text, /*department,*/ page, pageSize);
        }

        public async Task<ManagerProfileDto> GetManagerProfileAsync(Guid userIdentityId)
        {
            return await _identityService.GetManagerProfileAsync(userIdentityId);
        }

        public async Task<bool> UpdateManagerProfileAsync(ManagerUpdateProfileDto dto)
        {
            return await _identityService.UpdateManagerProfileAsync(dto);
        }

        public async Task<DoctorDto> GetDoctorByIdAsync(Guid doctorId)
        {
            return await _identityService.GetDoctorByIdAsync(doctorId);
        }

        public async Task<DoctorDto> ManagerUpdateDoctorAsync(Guid doctorId, ManagerDoctorUpdateDTO dto)
        {
            return await _identityService.ManagerUpdateDoctorAsync(doctorId, dto);
        }

        public async Task<Guid> CreateDoctorAsync(DoctorCreateDto dto)
        {
            return await _identityService.CreateDoctorAsync(dto);
        }

        public async Task DeleteDoctorAsync(Guid doctorId)
        {
            await _identityService.DeleteDoctorAsync(doctorId);
        }
    }
}
