using System;
using System.Threading.Tasks;
using System.Transactions;
using MediAppointment.Domain.Entities;

namespace MediAppointment.Application.Interfaces
{
    public interface IWalletService
    {
        Task DepositAsync(Guid userId, decimal amount, string paymentProviderTransactionId);
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<List<WalletTransaction>> GetTransactionsAsync(Guid userId);
        Task RefundAsync(Guid userId, decimal amount, string reason, string? relatedTransactionId = null);

    }
}
