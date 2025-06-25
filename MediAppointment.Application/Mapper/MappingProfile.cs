using AutoMapper;
using MediAppointment.Application.DTOs.AppointmentDTOs;
using MediAppointment.Application.DTOs.RoomTimeSlotDTOs;
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
            .ForMember(dest => dest.RoomName,
                       opt => opt.MapFrom(src => src.RoomTimeSlot.Room.Name))
            .ForMember(dest => dest.Time,
                       opt => opt.MapFrom(src => src.RoomTimeSlot.TimeSlot.TimeStart.ToString(@"hh\:mm")));

            // RoomTimeSlot mapping
            CreateMap<RoomTimeSlot, RoomTimeSlotResponse>()
                    .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room.Name))
                    .ForMember(dest => dest.TimeStart, opt => opt.MapFrom(src => src.TimeSlot.TimeStart))
                    .ForMember(dest => dest.TimeEnd, opt => opt.MapFrom(src => src.TimeSlot.TimeEnd))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
