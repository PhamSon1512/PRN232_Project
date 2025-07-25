namespace MediAppointment.Application.Interfaces
{
    public interface IGeminiChatService
    {
        Task<string> SendMessageAsync(string message, Guid? userId, CancellationToken cancellationToken = default);
        Task<string> GetDoctorsByDayAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<string> GetAllDepartmentsAsync(CancellationToken cancellationToken = default);
        Task<string> GetDoctorInfoAsync(string doctorName, CancellationToken cancellationToken = default);
        Task<string> GetAppointmentHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
