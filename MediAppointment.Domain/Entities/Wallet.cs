using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediAppointment.Domain.Entities.Abstractions;

namespace MediAppointment.Domain.Entities
{
    public class Wallet : Entity
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public List<WalletTransaction> Transactions { get; set; } = new();
    }
}
