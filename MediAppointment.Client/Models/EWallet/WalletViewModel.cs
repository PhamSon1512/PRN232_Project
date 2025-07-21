using MediAppointment.Client.Services;

namespace MediAppointment.Client.Models.EWallet
{
    internal class WalletViewModel
    {
        public decimal Balance { get; set; }
        public List<WalletTransaction> Transactions { get; set; }
        public bool IsConnected { get; set; }
        public string ErrorMessage { get; set; }
    }
}