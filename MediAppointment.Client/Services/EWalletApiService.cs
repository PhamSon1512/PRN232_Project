using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace MediAppointment.Client.Services
{
    public interface IWalletApiService
    {
        Task<decimal> GetBalanceAsync();
        Task<List<WalletTransaction>> GetTransactionsAsync();
        Task<string> CreatePaymentUrlAsync(decimal amount);
        Task<bool> DepositAsync(Guid userId, decimal amount, string transactionId);
        Task<bool> ReductAsync(decimal amount);

    }

    public class WalletApiService : BaseApiService, IWalletApiService
    {
        private readonly ILogger<WalletApiService> _logger;
        private readonly string _baseUrl;

        public WalletApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<WalletApiService> logger)
            : base(httpClient, httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = configuration["ApiBaseUrl"]!;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                SetAuthHeader();

                var response = await _httpClient.GetAsync("/api/wallet/balance");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get balance: {StatusCode} - {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                
                var result = JsonSerializer.Deserialize<BalanceResponse>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Balance ?? 0;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when getting balance");
                throw new ApplicationException("Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout when getting balance");
                throw new ApplicationException("Yêu cầu quá thời gian chờ. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when getting balance");
                throw new ApplicationException("Có lỗi xảy ra khi lấy số dư.");
            }
        }

        public async Task<List<WalletTransaction>> GetTransactionsAsync()
        {
            try
            {
                SetAuthHeader();

                var response = await _httpClient.GetAsync("/api/wallet/transactions");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get transactions: {StatusCode} - {ReasonPhrase}",
                        response.StatusCode, response.ReasonPhrase);
                    return new List<WalletTransaction>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var transactions = JsonSerializer.Deserialize<List<WalletTransaction>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return transactions ?? new List<WalletTransaction>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when getting transactions");
                throw new ApplicationException("Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout when getting transactions");
                throw new ApplicationException("Yêu cầu quá thời gian chờ. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when getting transactions");
                throw new ApplicationException("Có lỗi xảy ra khi lấy lịch sử giao dịch.");
            }
        }

        public async Task<string> CreatePaymentUrlAsync(decimal amount)
        {
            try
            {
                SetAuthHeader();


                var payload = new
                {
                    OrderType = "deposit",  
                    Amount = amount,
                    OrderDescription = $"Deposit|{amount}|{DateTime.UtcNow:O}",
                    Name = "Nạp tiền vào ví" 
                };


                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/payments/create-payment-url", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to create payment URL: {StatusCode} - {ErrorContent}",
                        response.StatusCode, errorContent);
                    throw new ApplicationException($"Không thể tạo URL thanh toán: {response.StatusCode}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentUrlResponse>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.PaymentUrl ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when creating deposit");
                throw new ApplicationException("Không thể kết nối đến server. Vui lòng kiểm tra kết nối mạng.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout when creating deposit");
                throw new ApplicationException("Yêu cầu quá thời gian chờ. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating deposit");
                throw new ApplicationException("Có lỗi xảy ra khi tạo giao dịch nạp tiền.");
            }
        }

        public async Task<bool> DepositAsync(Guid userId, decimal amount, string transactionId)
        {
            try
            {
                SetAuthHeader();

                var payload = new
                {
                    UserId = userId,
                    Amount = amount,
                    TransactionId = transactionId
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/wallet/deposit", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Deposit failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when calling deposit");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when completing deposit");
                return false;
            }
        }

        public async Task<bool> ReductAsync(decimal amount)
        {
            try
            {
                SetAuthHeader();

                var payload = new
                {
                    Amount = amount,
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/wallet/reduct", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Reduct failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed when calling reduct");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when completing reduct");
                return false;
            }
        }
    }

    // DTOs
    public class BalanceResponse
    {
        public decimal Balance { get; set; }
    }

    public class PaymentUrlResponse
    {
        public string PaymentUrl { get; set; } = string.Empty;
    }

    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
