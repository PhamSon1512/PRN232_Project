using MediAppointment.Application.DTOs.RoomTimeSlotDTOs;
using MediAppointment.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediAppointment.Application.Interfaces
{
    public interface IRoomTimeSlotService
    {

        /// Lấy danh sách RoomTimeSlot được phân công cho bác sĩ
        Task<IEnumerable<RoomTimeSlotResponse>> GetAssignedSlotsByDoctor(Guid doctorId, int? year = null, int? week = null);

        /// Lấy RoomTimeSlot theo ID
        Task<RoomTimeSlotResponse?> GetByIdAsync(Guid roomTimeSlotId);

        /// Cập nhật trạng thái RoomTimeSlot
        Task UpdateStatusAsync(Guid roomTimeSlotId, RoomTimeSlotStatus newStatus);

        // Lấy Thông tin chi tiết theo ID
        Task<RoomTimeSlotDetailResponse?> GetDetailWithAppointmentsAsync(Guid roomTimeSlotId);

    }
}
