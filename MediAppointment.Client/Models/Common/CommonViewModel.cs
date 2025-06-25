namespace MediAppointment.Client.Models.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class NotificationViewModel
    {
        public string Type { get; set; } = "info"; // success, error, warning, info
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
