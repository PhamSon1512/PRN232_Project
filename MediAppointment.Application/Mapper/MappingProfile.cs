using AutoMapper;
using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Application.DTOs.MedicalRecordDtos;
using MediAppointment.Application.DTOs.RoomTimeSlotDTOs;
using MediAppointment.Application.DTOs.TimeSlotDTOs;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Add your mappings here
            // Example: CreateMap<Source, Destination>();
            CreateMap<Appointment, AppointmentResponse>()
                .ForMember(dest => dest.RoomTimeSlotId,
                           opt => opt.MapFrom(src => src.RoomTimeSlotId))
                .ForMember(dest => dest.RoomName,
                           opt => opt.MapFrom(src => src.RoomTimeSlot.Room.Name))
                .ForMember(dest => dest.TimeSlot,
                           opt => opt.MapFrom(src => $"{src.RoomTimeSlot.TimeSlot.TimeStart:hh\\:mm} - {(src.RoomTimeSlot.TimeSlot.TimeStart.Add(src.RoomTimeSlot.TimeSlot.Duration)):hh\\:mm}"))
                .ForMember(dest => dest.Department,
                           opt => opt.MapFrom(src => src.RoomTimeSlot.Room.Department.DepartmentName))
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PatientName,
                           opt => opt.MapFrom(src => "")); // Empty for now

            // RoomTimeSlot mapping
            CreateMap<RoomTimeSlot, RoomTimeSlotResponse>()
                    .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.Name))
                    .ForMember(dest => dest.TimeStart, opt => opt.MapFrom(src => src.TimeSlot.TimeStart))
                    .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeSlot.TimeEnd))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                    .ForMember(dest => dest.Shift, opt => opt.MapFrom(src => src.TimeSlot.Shift ? "Afternoon" : "Morning")); 

            // MedicalRecord -> MedicalRecordViewDto
            CreateMap<MedicalRecord, MedicalRecordViewDto>();

            // CreateMedicalRecordDto -> MedicalRecord
            CreateMap<CreateMedicalRecordDto, MedicalRecord>();

            CreateMap<TimeSlot, TimeSlotDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TimeStart, opt => opt.MapFrom(src => src.TimeStart))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.Shift, opt => opt.MapFrom(src => src.Shift));
        }
    }
}
