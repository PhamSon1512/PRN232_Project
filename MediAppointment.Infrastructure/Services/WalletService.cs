using MediAppointment.Application.Interfaces;
using MediAppointment.Domain.Entities;
using MediAppointment.Infrastructure.Data;
using MediAppointment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediAppointment.Infrastructure.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _dbContext;

        public WalletService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DepositAsync(Guid userId, decimal amount, string paymentProviderTransactionId)
        {
            var wallet = await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 0
                };
                _dbContext.Wallets.Add(wallet);
            }

            wallet.Balance += amount;
            wallet.Transactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = "Deposit",
                Description = $"Deposit via VNPAY: {paymentProviderTransactionId}"
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            return wallet?.Balance ?? 0;
        }

        public async Task RefundAsync(Guid userId, decimal amount, string reason, string? relatedTransactionId = null)
        {
            var wallet = await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                throw new InvalidOperationException("Wallet not found.");

            wallet.Balance += amount;
            wallet.Transactions.Add(new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = "Refund",
                Description = $"Refund: {reason}" + (relatedTransactionId != null ? $" (Related Txn: {relatedTransactionId})" : "")
            });

            await _dbContext.SaveChangesAsync();
        }

    }
}
