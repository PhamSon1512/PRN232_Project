namespace MediAppointment.Application.Interfaces
{
    public interface IGeminiChatService
    {
        Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    }
}
