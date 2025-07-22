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
                // Đảm bảo wallet tồn tại trước khi thêm transaction
                await _dbContext.SaveChangesAsync();
            }

            wallet.Balance += amount;

            // Thêm trực tiếp vào DbSet nếu bạn gặp lỗi tracking quan hệ
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Amount = amount,
                Date = DateTime.UtcNow,
                Type = "DEPOSIT", // Nên dùng enum hoặc constant cho thống nhất
                Description = $"Deposit via VNPAY: {paymentProviderTransactionId}"
            };

            _dbContext.WalletTransactions.Add(transaction);

            await _dbContext.SaveChangesAsync();
        }


        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            return wallet?.Balance ?? 0;
        }

        public async Task<List<WalletTransaction>> GetTransactionsAsync(Guid userId)
        {
            var wallet = await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                return new List<WalletTransaction>();

            return wallet.Transactions
                .OrderByDescending(t => t.Date)
                .ToList();
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

        public async Task ReductAsync(Guid userId, decimal amount)
        {
            var wallet = await _dbContext.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
                throw new InvalidOperationException("Wallet not found.");
            try
            {
                wallet.Balance -= amount;

                var transaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    Amount = amount,
                    Date = DateTime.UtcNow,
                    Type = "DEDUCT", // Nên dùng enum hoặc constant cho thống nhất
                    Description = $"Deduct via EWallet"
                };

                _dbContext.WalletTransactions.Add(transaction);

                await _dbContext.SaveChangesAsync();

            } catch(Exception ex)
            {
                throw new Exception("Lỗi khi trừ tiền khỏi ví: " + ex.Message, ex);
            }
        }
    }
}
